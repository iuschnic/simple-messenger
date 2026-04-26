using BL.Contracts;
using BL.Events;
using BL.Exceptions;
using BL.Models;
using BL.Interfaces;

namespace BL.Services;

public class MessengerService : IMessengerService
{
    private readonly IHttpClient _http;
    private readonly IRealtimeClient _rt;
    private readonly RepositoryHub _db;

    public MessengerEvents Events { get; } = new();

    public MessengerService(
        IHttpClient http,
        IRealtimeClient rt,
        RepositoryHub db)
    {
        _http = http;
        _rt = rt;
        _db = db;

        _rt.MessageReceived += OnMessageReceived;
        _rt.UserLeftChat += OnUserLeftChat;
        _rt.ChatCreated += OnChatCreated;
        _rt.ReconnectedToHub += OnReconnectedToHub;
    }

    // ================= ERROR WRAPPER =================

    private async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (HubConnectionException ex)
        {
            throw new HubConnectionException(
                $"Ошибка соединения с хабом: {ex.Message}"
            );
        }
        catch (ApiException ex)
        {
            throw ExceptionMapper.Map(ex);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Ошибка базы данных: {ex.Message}");
        }
    }

    private async Task Execute(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (HubConnectionException ex)
        {
            throw new HubConnectionException(
                $"Ошибка соединения с хабом: {ex.Message}"
            );
        }
        catch (ApiException ex)
        {
            throw ExceptionMapper.Map(ex);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Ошибка базы данных: {ex.Message}");
        }
    }

    // ================= INTERNAL =================

    private async Task SyncFullChat(Guid chatId)
    {
        var chat = await Execute(() => _db.Chats.Find(chatId));
        var version = chat?.Version ?? 0;

        var sync = await Execute(() =>
            _http.SyncChat(chatId, version)
        );

        foreach (var u in sync.Participants)
        {
            try
            {
                await Execute(() => _db.Users.Save(u));
            }
            catch{}
        }
        
        chat.Version = sync.LastVersion;
        chat.Name = sync.ChatName;
        chat.CreatedAt = sync.CreatedAt;
        chat.OwnerId = sync.OwnerId;
        chat.Type = sync.ChatType;

        if (sync.Messages.Any())
            chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);

        await Execute(() => _db.Chats.Save(chat));
        
        foreach (var u in sync.Participants)
        {
            try
            {
                await Execute(() => _db.Chats.AddUserToChat(chat.Id, u.Id));
            }
            catch{}
        }

        foreach (var m in sync.Messages)
        {
            try
            {
                await Execute(() => _db.Messages.Save(m));
            }
            catch{}
        }
    }

    public async Task SyncFullChats()
    {
        var chats = (await Execute(() => _db.Chats.GetAllChats())) 
                    ?? new List<Chat>();

        var request = chats
            .Select(c => (c.Id, c.Version))
            .ToList();

        var syncResults = await Execute(() => _http.SyncChats(request));

        foreach (var sync in syncResults)
        {
            var chat = chats.FirstOrDefault(c => c.Id == sync.ChatId);
            
            if (chat == null)
                chat = new Chat { Id = sync.ChatId };

            chat.Version = sync.LastVersion;
            chat.Name = sync.ChatName;
            chat.CreatedAt = sync.CreatedAt;
            chat.OwnerId = sync.OwnerId;
            chat.Type = sync.ChatType;

            if (sync.Messages.Count != 0)
                chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);

            foreach (var u in sync.Participants)
            {
                try
                {
                    await Execute(() => _db.Users.Save(u));
                }
                catch{}
            }
            
            await Execute(() => _db.Chats.Save(chat));
            
            foreach (var u in sync.Participants)
            {
                try
                {
                    await Execute(() => _db.Chats.AddUserToChat(chat.Id, u.Id));
                }
                catch{}
            }
            
            foreach (var m in sync.Messages)
            {
                try
                {
                    await Execute(() => _db.Messages.Save(m));
                }
                catch{}
            }
        }
    }

    
    private async Task OnMessageReceived(Message message)
    {
        if (await Execute(() => _db.Messages.Find(message.MessageNumber, message.ChatId)) != null)
            return;

        var chat = await Execute(() => _db.Chats.Find(message.ChatId));

        if (chat == null || message.Version > chat.Version + 1)
        {
            await SyncFullChats();
            return;
        }

        try
        {
            await Execute(() => _db.Messages.Save(message));
        }
        catch
        {
            return;
        }

        chat.LastMessageNum = message.MessageNumber;
        chat.Version = message.Version;

        await Execute(() => _db.Chats.Save(chat));

        await Events.RaiseMessageReceived(message);
    }

    private async Task OnUserLeftChat(Guid chatId, Guid userId)
    {
        await Execute(() => _db.Chats.RemoveUserFromChat(chatId, userId));
        await  Events.RaiseUserLeftChat(chatId, userId);
    }

    private async Task OnChatCreated(Chat chat)
    {
        if (await Execute(() => _db.Chats.Find(chat.Id)) != null)
            return;

        try
        {
            await Execute(() => _db.Chats.Save(chat));
        }
        catch
        {
            return;
        }

        foreach (var p in chat.Members)
        {
            var user = await GetUserById(p.Id) ?? await Execute(() => _db.Users.Save(p));
            await Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        await SyncFullChat(chat.Id);
        await Events.RaiseChatCreated(chat);
    }
    
    private async Task OnReconnectedToHub()
    {
        await SyncFullChats();
        await Events.RaiseReconnectedToHub();
    }

    // ================= AUTH =================

    public async Task<CurrentUser> RegisterUser(string u, string p, string e, string d)
    {
        await Execute(() => _http.Register(u, p, e, d));

        return await Execute(() => _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            Email = e,
            PasswordHash = p,
            DisplayedName = d
        }));
    }

    public async Task<ReturnCode> LoginAgain()
    {
        var u = await Execute(() =>  _db.CurrentUser.Get());
        if (u == null)
        {
            return ReturnCode.Error;
        }
        
        var token = await Execute(() => _http.Login(u.UniqueName, u.PasswordHash));
        
        await Execute(() => _rt.ConnectToHub(token));
        
        return ReturnCode.Success;
    }

    public async Task<User> Login(string u, string p)
    {
        var token = await Execute(() => _http.Login(u, p));

        await Execute(() => _rt.ConnectToHub(token));

        var user = await Execute(() => _http.GetMe());
        
        await Execute(() => _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            PasswordHash = p,
            DisplayedName = user.DisplayName
        }));

        return await Execute(() => _db.Users.Save(new User
        {
            Id = user.Id,
            UniqueName = user.UniqueName,
            DisplayName = user.DisplayName
        }));
    }

    public async Task<CurrentUser> UpdateMeDisplayName(Guid id, string displayName)
    {
        await Execute(() => _http.UpdateMeDisplayName(displayName));

        var user = await Execute(() => _db.CurrentUser.Get());

        if (user != null)
        {
            user.DisplayedName = displayName;
            await Execute(() => _db.CurrentUser.Save(user));
        }

        return user!;
    }

    public async Task<CurrentUser?> GetCurrentUser()
        => await Execute(() => _db.CurrentUser.Get());

    // ================= USERS =================

    public async Task<User> GetUserByNameWithServer(string uniqueName)
    {
        var user = await Execute(() => _db.Users.FindByUniqueName(uniqueName));

        if (user == null)
        {
            user = await Execute(() => _http.GetUserByName(uniqueName));
            await Execute(() => _db.Users.Save(user));
        }

        return user;
    }

    public async Task<User?> GetUserById(Guid id)
        => await Execute(() => _db.Users.Find(id));

    public async Task<User> UpdateContactName(Guid id, string contact)
    {
        await Execute(() => _http.UpdateContactName(id, contact));

        var user = await Execute(() => _db.Users.Find(id));

        if (user != null)
        {
            user.ContactName = contact;
            await Execute(() => _db.Users.Save(user));
        }

        return user!;
    }

    public async Task<List<User>> FindUsersWithContactName()
        => await Execute(() => _db.Users.FindUsersWithContactName());

    public async Task<User?> FindUsersByUniqueName(string uniqueName)
        => await Execute(() => _db.Users.FindByUniqueName(uniqueName));

    // ================= CHATS =================

    public async Task<List<Chat>> GetAllChats()
        => await Execute(() => _db.Chats.GetAllChats());

    public async Task<Chat> CreateGroupChat(string name, Guid creatorId, List<Guid> participants)
    {
        var chatHttp = await Execute(() => _http.CreateGroupChat(name, participants));

        var chat = new Chat
        {
            Id = chatHttp.Id,
            OwnerId = chatHttp.OwnerId,
            Name = chatHttp.Name,
            CreatedAt = chatHttp.CreatedAt,
            Version = 0,
            Type = chatHttp.Type,
            LastMessageNum = chatHttp.LastMessageNum,
        };

        await Execute(() => _db.Chats.Save(chat));

        foreach (var p in participants)
        {
            var user = await GetUserById(p);
            if (user != null)
                await Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        await Execute(() => SyncFullChat(chat.Id));

        return chat;
    }

    public async Task<Chat> CreatePrivateChat(Guid creatorId, List<Guid> participants)
    {
        var chatHttp = await Execute(() => _http.CreatePrivateChat(participants.Last()));

        var chat = new Chat
        {
            Id = chatHttp.Id,
            OwnerId = chatHttp.OwnerId,
            Name = chatHttp.Name,
            CreatedAt = chatHttp.CreatedAt,
            Version = 0,
            Type = chatHttp.Type,
            LastMessageNum = chatHttp.LastMessageNum,
        };

        await Execute(() => _db.Chats.Save(chat));

        foreach (var p in participants)
        {
            var user = await GetUserById(p);
            if (user != null)
                await Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        return chat;
    }

    public async Task<List<User>> GetChatParticipants(Guid chatId)
        => await Execute(() => _db.Chats.FindChatUsers(chatId));

    public async Task AddUserToChat(Guid chatId, string uniqueName)
    {
        var user = await Execute(() => _db.Users.FindByUniqueName(uniqueName));
        if (user != null)
            await Execute(() => _db.Chats.AddUserToChat(chatId, user.Id));
    }

    public async Task LeaveChat(Guid chatId, Guid userId)
    {
        var chat = await Execute(() => _db.Chats.Find(chatId));
        await Execute(() => _http.RemoveUserFromChat(chatId, userId, chat!.Version));
        await Execute(() => _db.Chats.LeaveAndDeleteChat(chatId, userId));
    }

    // ================= MESSAGES =================

    public async Task<List<Message>> GetChatMessages(Guid chatId)
    {
        var list = await Execute(() => _db.Messages.FindChatMessages(chatId));

        if (list.Any())
        {
            await Execute(() => _db.Chats.UpdateLastMessageNum(
                chatId,
                list.Last().MessageNumber
            ));
        }

        return list;
    }

    public async Task<Message> SendMessage(Guid chatId, Guid senderId, string text)
    {
        var chat = await Execute(() => _db.Chats.Find(chatId))
                   ?? throw new NotFoundAppException("Chat not found");
        
        var sync = await Execute(() => _http.SendMessage(chatId, text, chat.Version));

        if (sync.Messages == null || sync.Messages.Count == 0)
            throw new AppException("No messages returned from server");

        foreach (var m in sync.Messages)
        {
            if (await Execute(() => _db.Messages.Find(m.MessageNumber, sync.ChatId)) == null)
            {
                Console.WriteLine($"SendMessage {m.MessageNumber} {m.ChatId}");
                try
                {
                    await Execute(() => _db.Messages.Save(m));
                }
                catch
                {}
            }
        }

        chat.Version = sync.LastVersion;

        var lastMsg = sync.Messages
            .OrderBy(m => m.MessageNumber)
            .Last();

        chat.LastMessageNum = lastMsg.MessageNumber;

        await Execute(() => _db.Chats.Save(chat));

        return lastMsg;
    }

    public async Task UpdateLastReadMessageNum(Guid chatId, Guid userId)
    {
        var messages = await GetChatMessages(chatId);

        if (messages.Any())
        {
            await Execute(() => _db.Chats.UpdateLastReadMessageNum(
                chatId,
                userId,
                messages.Last().MessageNumber
            ));
        }
    }
}