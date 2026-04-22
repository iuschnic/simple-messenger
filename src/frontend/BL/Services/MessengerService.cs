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
    }

    // ================= INTERNAL =================

    private async Task SyncFullChat(Guid chatId)
    {
        try
        {
            var chat = await _db.Chats.Find(chatId);
            var version = chat?.Version ?? 0;

            var sync = (await _http.SyncChats(new List<(Guid, ulong)> { (chatId, version) }))
                .First();

            foreach (var m in sync.Messages)
                await _db.Messages.Save(m);

            if (chat == null)
                chat = new Chat { Id = chatId };

            chat.Version = sync.LastVersion;

            if (sync.Messages.Any())
                chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);

            await _db.Chats.Save(chat);
        }
        catch (ApiException ex)
        {
            throw ExceptionMapper.Map(ex);
        }
    }

    private async void OnMessageReceived(Message message)
    {
        try
        {
            if (await _db.Messages.Find(message.MessageNumber) != null)
                return;

            var chat = await _db.Chats.Find(message.ChatId);

            if (chat == null || message.Version > chat.Version + 1)
            {
                await SyncFullChat(message.ChatId);
                return;
            }

            await _db.Messages.Save(message);

            chat.LastMessageNum = message.MessageNumber;
            chat.Version = message.Version;

            await _db.Chats.Save(chat);

            Events.RaiseMessageReceived(message);
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Ошибка: {ex.Message}");
        }
    }

    private async void OnUserLeftChat(Guid chatId, Guid userId)
    {
        await _db.Chats.RemoveUserFromChat(chatId, userId);
        Events.RaiseUserLeftChat(chatId, userId);
    }

    private async void OnChatCreated(Chat chat)
    {
        await _db.Chats.Save(chat);

        foreach (var p in chat.Members)
        {
            var user = await _db.Users.Find(p.Id) ?? await _db.Users.Save(p);
            await _db.Chats.AddUserToChat(chat.Id, user.Id);
        }

        Events.RaiseChatCreated(chat);
    }

    // ================= AUTH =================

    public async Task<CurrentUser> RegisterUser(string u, string p, string e, string d)
    {
        await _http.Register(u, p, e, d);

        return await _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            Email = e,
            PasswordHash = p,
            DisplayedName = d
        });
    }

    public async Task<User> Login(string u, string p)
    {
        var local = await _db.CurrentUser.Get();

        if (local == null || u != local.UniqueName || p != local.PasswordHash)
            throw new AuthException("Неверный логин или пароль");

        var token = await _http.Login(u, p);

        _rt.ConnectToHub(token);

        var user = await _http.GetMe();

        return await _db.Users.Save(new User
        {
            Id = user.Id,
            UniqueName = user.UniqueName,
            DisplayName = user.DisplayName
        });
    }

    public async Task<CurrentUser> UpdateMeDisplayName(Guid id, string displayName)
    {
        await _http.UpdateMeDisplayName(displayName);

        var user = await _db.CurrentUser.Get();

        if (user != null)
        {
            user.DisplayedName = displayName;
            await _db.CurrentUser.Save(user);
        }

        return user!;
    }

    public async Task<CurrentUser?> GetCurrentUser()
        => await _db.CurrentUser.Get();

    // ================= USERS =================

    public async Task<User> GetUserByNameWithServer(string uniqueName)
    {
        var user = await _db.Users.FindByUniqueName(uniqueName);

        if (user == null)
        {
            user = await _http.GetUserByName(uniqueName);
            await _db.Users.Save(user);
        }

        return user;
    }

    public async Task<User?> GetUserById(Guid id)
        => await _db.Users.Find(id);

    public async Task<User> UpdateContactName(Guid id, string contact)
    {
        await _http.UpdateContactName(id, contact);

        var user = await _db.Users.Find(id);

        if (user != null)
        {
            user.ContactName = contact;
            await _db.Users.Save(user);
        }

        return user!;
    }

    public async Task<List<User>> FindUsersWithContactName()
        => await _db.Users.FindUsersWithContactName();

    public async Task<User?> FindUsersByUniqueName(string uniqueName)
        => await _db.Users.FindByUniqueName(uniqueName);

    // ================= CHATS =================

    public async Task<List<Chat>> GetAllChats()
        => await _db.Chats.GetAllChats();

    public async Task<Chat> CreateGroupChat(string name, Guid creatorId, List<Guid> participants)
    {
        var chatHttp = await _http.CreateGroupChat(name, participants);

        var chat = new Chat
        {
            Id = chatHttp.Id,
            OwnerId = chatHttp.OwnerId,
            Name = chatHttp.Name,
            Version = chatHttp.Version,
            Type = chatHttp.Type,
            LastMessageNum = chatHttp.LastMessageNum,
        };

        await _db.Chats.Save(chat);

        foreach (var p in participants)
        {
            var user = await _db.Users.Find(p);
            if (user != null)
                await _db.Chats.AddUserToChat(chat.Id, user.Id);
        }

        return chat;
    }

    public async Task<Chat> CreatePrivateChat(Guid creatorId, List<Guid> participants)
    {
        var chatHttp = await _http.CreatePrivateChat(participants.Last());

        var chat = new Chat
        {
            Id = chatHttp.Id,
            OwnerId = chatHttp.OwnerId,
            Name = chatHttp.Name,
            CreatedAt = chatHttp.CreatedAt,
            Version = chatHttp.Version,
            Type = chatHttp.Type,
            LastMessageNum = chatHttp.LastMessageNum,
        };

        await _db.Chats.Save(chat);

        foreach (var p in participants)
        {
            var user = await _db.Users.Find(p);
            if (user != null)
                await _db.Chats.AddUserToChat(chat.Id, user.Id);
        }

        return chat;
    }

    public async Task<List<User>> GetChatParticipants(Guid chatId)
        => await _db.Chats.FindChatUsers(chatId);

    public async Task AddUserToChat(Guid chatId, string uniqueName)
    {
        var user = await _db.Users.FindByUniqueName(uniqueName);
        if (user != null)
            await _db.Chats.AddUserToChat(chatId, user.Id);
    }

    public async Task LeaveChat(Guid chatId, Guid userId)
    {
        await _http.RemoveUserFromChat(chatId, userId);
        await _db.Chats.RemoveUserFromChat(chatId, userId);
    }

    // ================= MESSAGES =================

    public async Task<List<Message>> GetChatMessages(Guid chatId)
    {
        var list = await _db.Messages.FindChatMessages(chatId);

        if (list.Any())
        {
            await _db.Chats.UpdateLastMessageNum(
                chatId,
                list.Last().MessageNumber
            );
        }

        return list;
    }

    public async Task<Message> SendMessage(Guid chatId, Guid senderId, string text)
    {
        var chat = await _db.Chats.Find(chatId)
                   ?? throw new NotFoundAppException("Chat not found");

        var sync = await _http.SendMessage(chatId, text, chat.Version);

        if (sync.Messages == null || sync.Messages.Count == 0)
            throw new AppException("No messages returned from server");

        foreach (var m in sync.Messages)
        {
            if (await _db.Messages.Find(m.MessageNumber) == null)
                await _db.Messages.Save(m);
        }

        chat.Version = sync.LastVersion;

        var lastMsg = sync.Messages
            .OrderBy(m => m.MessageNumber)
            .Last();

        chat.LastMessageNum = lastMsg.MessageNumber;

        await _db.Chats.Save(chat);

        return lastMsg;
    }

    public async Task UpdateLastReadMessageNum(Guid chatId, Guid userId)
    {
        var messages = await GetChatMessages(chatId);

        if (messages.Any())
        {
            await _db.Chats.UpdateLastReadMessageNum(
                chatId,
                userId,
                messages.Last().MessageNumber
            );
        }
    }
}