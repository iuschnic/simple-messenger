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
    }

    // ================= INTERNAL =================

    private void SyncFullChat(Guid chatId)
    {
        var chat = _db.Chats.Find(chatId);
        var version = chat?.Version ?? 0;

        var sync = Execute(() =>
            _http.SyncChats(new List<(Guid, ulong)>
            {
                (chatId, version)
            }).First()
        );

        foreach (var m in sync.Messages)
            _db.Messages.Save(m);

        if (chat == null)
            chat = new Chat { Id = chatId };

        chat.Version = sync.LastVersion;

        if (sync.Messages.Any())
            chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);

        _db.Chats.Save(chat);
    }

    private void OnMessageReceived(Message message)
    {
        if (_db.Messages.Find(message.MessageNumber) != null)
            return;

        var chat = _db.Chats.Find(message.ChatId);

        if (chat == null || message.Version > chat.Version + 1)
        {
            SyncFullChat(message.ChatId);
            return;
        }

        _db.Messages.Save(message);

        chat.LastMessageNum = message.MessageNumber;
        chat.Version = message.Version;

        _db.Chats.Save(chat);

        Events.RaiseMessageReceived(message);
    }

    private void OnUserLeftChat(Guid chatId, Guid userId)
    {
        _db.Chats.RemoveUserFromChat(chatId, userId);
        Events.RaiseUserLeftChat(chatId, userId);
    }

    private void OnChatCreated(Chat chat)
    {
        _db.Chats.Save(chat);

        foreach (var p in chat.Members)
        {
            var user = GetUserById(p.Id) ?? _db.Users.Save(p);
            _db.Chats.AddUserToChat(chat.Id, user.Id);
        }

        Events.RaiseChatCreated(chat);
    }

    // ================= AUTH =================

    public CurrentUser RegisterUser(string u, string p, string e, string d)
    {
        Execute(() => _http.Register(u, p, e, d));

        return _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            Email = e,
            PasswordHash = p,
            DisplayedName = d
        });
    }

    public User Login(string u, string p)
    {
        var local = _db.CurrentUser.Get();

        if (local == null || u != local.UniqueName || p != local.PasswordHash)
            throw new Exception("Incorrect username or password");

        var token = Execute(() => _http.Login(u, p));

        _rt.ConnectToHub(token);

        var user = Execute(() => _http.GetMe());

        return _db.Users.Save(new User
        {
            Id = user.Id,
            UniqueName = user.UniqueName,
            DisplayName = user.DisplayName
        });
    }

    public CurrentUser UpdateMeDisplayName(Guid id, string displayName)
    {
        Execute(() => _http.UpdateMeDisplayName(displayName));

        var user = _db.CurrentUser.Get();
        if (user != null)
        {
            user.DisplayedName = displayName;
            _db.CurrentUser.Save(user);
        }

        return user!;
    }

    public CurrentUser GetCurrentUser()
        => _db.CurrentUser.Get();

    // ================= USERS =================

    public User GetUserByNameWithServer(string uniqueName)
    {
        var user = _db.Users.FindByUniqueName(uniqueName);

        if (user == null)
        {
            user = Execute(() => _http.GetUserByName(uniqueName));
            _db.Users.Save(user);
        }

        return user;
    }

    public User? GetUserById(Guid id)
        => _db.Users.Find(id);

    public User UpdateContactName(Guid id, string contact)
    {
        Execute(() => _http.UpdateContactName(id, contact));

        var user = _db.Users.Find(id);
        if (user != null)
        {
            user.ContactName = contact;
            _db.Users.Save(user);
        }

        return user!;
    }

    public List<User> FindUsersWithContactName()
        => _db.Users.FindUsersWithContactName();

    public User FindUsersByUniqueName(string uniqueName)
        => _db.Users.FindByUniqueName(uniqueName);

    // ================= CHATS =================

    public List<Chat> GetAllChats()
        => _db.Chats.GetAllChats();

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

        _db.Chats.Save(chat);

        foreach (var p in participants)
        {
            var user = GetUserById(p);
            if (user != null)
                _db.Chats.AddUserToChat(chat.Id, user.Id);
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

        _db.Chats.Save(chat);

        foreach (var p in participants)
        {
            var user = GetUserById(p);
            if (user != null)
                _db.Chats.AddUserToChat(chat.Id, user.Id);
        }

        return chat;
    }

    public List<User> GetChatParticipants(Guid chatId)
        => _db.Chats.FindChatUsers(chatId);

    public void AddUserToChat(Guid chatId, string uniqueName)
    {
        var user = _db.Users.FindByUniqueName(uniqueName);
        if (user != null)
            _db.Chats.AddUserToChat(chatId, user.Id);
    }

    public void LeaveChat(Guid chatId, Guid userId)
    {
        Execute(() => _http.RemoveUserFromChat(chatId, userId));
        _db.Chats.RemoveUserFromChat(chatId, userId);
    }

    // ================= MESSAGES =================

    public List<Message> GetChatMessages(Guid chatId)
    {
        var list = _db.Messages.FindChatMessages(chatId);

        if (list.Any())
            _db.Chats.UpdateLastMessageNum(chatId, list.Last().MessageNumber);

        return list;
    }

    public Message SendMessage(Guid chatId, Guid senderId, string text)
    {
        var chat = _db.Chats.Find(chatId)
                   ?? throw new Exception("Chat not found");

        var sync = Execute(() => _http.SendMessage(chatId, text, chat.Version));

        if (sync.Messages == null || sync.Messages.Count == 0)
            throw new Exception("No messages returned from server");

        foreach (var m in sync.Messages)
        {
            if (_db.Messages.Find(m.MessageNumber) == null)
                _db.Messages.Save(m);
        }

        chat.Version = sync.LastVersion;

        var lastMsg = sync.Messages
            .OrderBy(m => m.MessageNumber)
            .Last();

        chat.LastMessageNum = lastMsg.MessageNumber;

        _db.Chats.Save(chat);

        return lastMsg;
    }

    public void UpdateLastReadMessageNum(Guid chatId, Guid userId)
    {
        var messages = GetChatMessages(chatId);

        if (messages.Any())
        {
            _db.Chats.UpdateLastReadMessageNum(
                chatId,
                userId,
                messages.Last().MessageNumber
            );
        }
    }
}