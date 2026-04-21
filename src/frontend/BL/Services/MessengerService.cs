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

    // ================= ERROR WRAPPER =================

    private T Execute<T>(Func<T> action)
    {
        try
        {
            return action();
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
            throw new DatabaseException($"DB error: {ex.Message}");
        }
    }

    private void Execute(Action action)
    {
        try
        {
            action();
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
            throw new DatabaseException($"DB error: {ex.Message}");
        }
    }

    // ================= INTERNAL =================

    private void SyncFullChat(Guid chatId)
    {
        var chat = Execute(() => _db.Chats.Find(chatId));
        var version = chat?.Version ?? 0;

        var sync = Execute(() =>
            _http.SyncChats(new List<(Guid, ulong)> { (chatId, version) }).First()
        );

        foreach (var m in sync.Messages)
            Execute(() => _db.Messages.Save(m));

        if (chat == null)
            chat = new Chat { Id = chatId };

        chat.Version = sync.LastVersion;

        if (sync.Messages.Any())
            chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);

        Execute(() => _db.Chats.Save(chat));
    }

    private void OnMessageReceived(Message message)
    {
        if (Execute(() => _db.Messages.Find(message.MessageNumber)) != null)
            return;

        var chat = Execute(() => _db.Chats.Find(message.ChatId));

        if (chat == null || message.Version > chat.Version + 1)
        {
            SyncFullChat(message.ChatId);
            return;
        }

        Execute(() => _db.Messages.Save(message));

        chat.LastMessageNum = message.MessageNumber;
        chat.Version = message.Version;

        Execute(() => _db.Chats.Save(chat));

        Events.RaiseMessageReceived(message);
    }

    private void OnUserLeftChat(Guid chatId, Guid userId)
    {
        Execute(() => _db.Chats.RemoveUserFromChat(chatId, userId));
        Events.RaiseUserLeftChat(chatId, userId);
    }

    private void OnChatCreated(Chat chat)
    {
        Execute(() => _db.Chats.Save(chat));

        foreach (var p in chat.Members)
        {
            var user = GetUserById(p.Id) ?? Execute(() => _db.Users.Save(p));
            Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        Events.RaiseChatCreated(chat);
    }

    // ================= AUTH =================

    public CurrentUser RegisterUser(string u, string p, string e, string d)
    {
        Execute(() => _http.Register(u, p, e, d));

        return Execute(() => _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            Email = e,
            PasswordHash = p,
            DisplayedName = d
        }));
    }

    public User Login(string u, string p)
    {
        var local = Execute(() => _db.CurrentUser.Get());

        if (local == null || u != local.UniqueName || p != local.PasswordHash)
            throw new AuthException("Неверный логин или пароль");

        var token = Execute(() => _http.Login(u, p));

        _rt.ConnectToHub(token);

        var user = Execute(() => _http.GetMe());

        return Execute(() => _db.Users.Save(new User
        {
            Id = user.Id,
            UniqueName = user.UniqueName,
            DisplayName = user.DisplayName
        }));
    }

    public CurrentUser UpdateMeDisplayName(Guid id, string displayName)
    {
        Execute(() => _http.UpdateMeDisplayName(displayName));

        var user = Execute(() => _db.CurrentUser.Get());

        if (user != null)
        {
            user.DisplayedName = displayName;
            Execute(() => _db.CurrentUser.Save(user));
        }

        return user!;
    }

    public CurrentUser GetCurrentUser()
        => Execute(() => _db.CurrentUser.Get());

    // ================= USERS =================

    public User GetUserByNameWithServer(string uniqueName)
    {
        var user = Execute(() => _db.Users.FindByUniqueName(uniqueName));

        if (user == null)
        {
            user = Execute(() => _http.GetUserByName(uniqueName));
            Execute(() => _db.Users.Save(user));
        }

        return user;
    }

    public User? GetUserById(Guid id)
        => Execute(() => _db.Users.Find(id));

    public User UpdateContactName(Guid id, string contact)
    {
        Execute(() => _http.UpdateContactName(id, contact));

        var user = Execute(() => _db.Users.Find(id));

        if (user != null)
        {
            user.ContactName = contact;
            Execute(() => _db.Users.Save(user));
        }

        return user!;
    }

    public List<User> FindUsersWithContactName()
        => Execute(() => _db.Users.FindUsersWithContactName());

    public User FindUsersByUniqueName(string uniqueName)
        => Execute(() => _db.Users.FindByUniqueName(uniqueName));

    // ================= CHATS =================

    public List<Chat> GetAllChats()
        => Execute(() => _db.Chats.GetAllChats());

    public Chat CreateGroupChat(string name, Guid creatorId, List<Guid> participants)
    {
        var chatHttp = Execute(() => _http.CreateGroupChat(name, participants));

        var chat = new Chat
        {
            Id = chatHttp.Id,
            OwnerId = chatHttp.OwnerId,
            Name = chatHttp.Name,
            Version = chatHttp.Version,
            Type = chatHttp.Type,
            LastMessageNum = chatHttp.LastMessageNum,
        };

        Execute(() => _db.Chats.Save(chat));

        foreach (var p in participants)
        {
            var user = GetUserById(p);
            if (user != null)
                Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        return chat;
    }

    public Chat CreatePrivateChat(Guid creatorId, List<Guid> participants)
    {
        var chatHttp = Execute(() => _http.CreatePrivateChat(participants.Last()));

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

        Execute(() => _db.Chats.Save(chat));

        foreach (var p in participants)
        {
            var user = GetUserById(p);
            if (user != null)
                Execute(() => _db.Chats.AddUserToChat(chat.Id, user.Id));
        }

        return chat;
    }

    public List<User> GetChatParticipants(Guid chatId)
        => Execute(() => _db.Chats.FindChatUsers(chatId));

    public void AddUserToChat(Guid chatId, string uniqueName)
    {
        var user = Execute(() => _db.Users.FindByUniqueName(uniqueName));
        if (user != null)
            Execute(() => _db.Chats.AddUserToChat(chatId, user.Id));
    }

    public void LeaveChat(Guid chatId, Guid userId)
    {
        Execute(() => _http.RemoveUserFromChat(chatId, userId));
        Execute(() => _db.Chats.RemoveUserFromChat(chatId, userId));
    }

    // ================= MESSAGES =================

    public List<Message> GetChatMessages(Guid chatId)
    {
        var list = Execute(() => _db.Messages.FindChatMessages(chatId));

        if (list.Any())
        {
            Execute(() => _db.Chats.UpdateLastMessageNum(
                chatId,
                list.Last().MessageNumber
            ));
        }

        return list;
    }

    public Message SendMessage(Guid chatId, Guid senderId, string text)
    {
        var chat = Execute(() => _db.Chats.Find(chatId))
                   ?? throw new NotFoundAppException("Chat not found");

        var sync = Execute(() => _http.SendMessage(chatId, text, chat.Version));

        if (sync.Messages == null || sync.Messages.Count == 0)
            throw new AppException("No messages returned from server");

        foreach (var m in sync.Messages)
        {
            if (Execute(() => _db.Messages.Find(m.MessageNumber)) == null)
                Execute(() => _db.Messages.Save(m));
        }

        chat.Version = sync.LastVersion;

        var lastMsg = sync.Messages
            .OrderBy(m => m.MessageNumber)
            .Last();

        chat.LastMessageNum = lastMsg.MessageNumber;

        Execute(() => _db.Chats.Save(chat));

        return lastMsg;
    }

    public void UpdateLastReadMessageNum(Guid chatId, Guid userId)
    {
        var messages = GetChatMessages(chatId);

        if (messages.Any())
        {
            Execute(() => _db.Chats.UpdateLastReadMessageNum(
                chatId,
                userId,
                messages.Last().MessageNumber
            ));
        }
    }
}