using BL.Contracts;
using BL.Events;
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

        // Подписка на события
        _rt.MessageReceived += OnMessageReceived;
        _rt.UserLeftChat += OnUserLeftChat;  // Подписка на событие выхода пользователя из чата
        _rt.ChatCreated += OnChatCreated;    // Подписка на событие создания чата
    }

    private void SyncFullChat(Guid chatId)
    {
        var chat = _db.Chats.Find(chatId);

        var version = chat?.Version ?? 0;

        var sync = _http.SyncChats(new List<(Guid, long)>
        {
            (chatId, version)
        }).First();

        // сохраняем все сообщения
        foreach (var m in sync.Messages)
        {
            _db.Messages.Save(m);
        }

        // обновляем чат
        if (chat == null)
        {
            chat = new Chat
            {
                Id = chatId
            };
        }

        chat.Version = sync.LastVersion;

        if (sync.Messages.Any())
        {
            chat.LastMessageNum = sync.Messages.Max(m => m.MessageNumber);
        }

        _db.Chats.Save(chat);
    }
    
    private void OnMessageReceived(Message message)
    {
        var exists = _db.Messages.Find(message.MessageNumber);
        if (exists != null)
            return;

        var chat = _db.Chats.Find(message.ChatId);

        //если нет чата — сразу фулл синк
        if (chat == null)
        {
            SyncFullChat(message.ChatId);
            return;
        }

        // если версия сообщения больше → рассинхрон
        if (message.Version > chat.Version + 1)
        {
            SyncFullChat(chat.Id);
            return;
        }

        // просто сохраняем
        _db.Messages.Save(message);

        // обновляем чат
        chat.LastMessageNum = message.MessageNumber;
        chat.Version = message.Version;

        _db.Chats.Save(chat);

        Events.RaiseMessageReceived(message);
    }

    private void OnUserLeftChat(Guid chatId, Guid userId)
    {
        _db.Chats.RemoveUserFromChat(chatId, userId);; // Пример для текущего пользователя
        Events.RaiseUserLeftChat(chatId, userId);  // Оповещаем UI
    }

    private void OnChatCreated(Chat chat)
    {
        _db.Chats.Save(chat);
        foreach (var p in chat.Members)
        {
            var user = GetUserById(p.Id);
            if (user is null) 
                user = _db.Users.Save(p);
            _db.Chats.AddUserToChat(chat.Id, user.Id);
        }
        
        Events.RaiseChatCreated(chat);  // Оповещаем UI
    }
    
    // ================= AUTH =================

    public CurrentUser RegisterUser(string u, string p, string e, string d)
    {
        var res = _http.Register(u, p, e, d);

        // 1. обработка статуса в BL
        if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new Exception("UniqueName already exists");

        if (res.StatusCode != System.Net.HttpStatusCode.Created)
        {
            var body = res.Content.ReadAsStringAsync().Result;
            throw new Exception($"Register failed: {(int)res.StatusCode} {body}");
        }

        // 4. сохраняем CurrentUser (сессию)
        CurrentUser user_create = _db.CurrentUser.Save(new CurrentUser
        {
            Id = Guid.NewGuid(),
            UniqueName = u,
            Email = e,
            PasswordHash = p,
            DisplayedName = d
        });
        
        // _db.Users.Save(new User
        // {
        //     Id = Guid.NewGuid(),
        //     UniqueName = u,
        //     DisplayName = d
        // });

        // 5. возвращаем UI
        return user_create;
    }
    public User Login(string u, string p)
    {
        CurrentUser a = _db.CurrentUser.Get();
        
        if (u != a.UniqueName ||  p != a.PasswordHash)
        {
            throw new Exception($"Incorrect username or password");
        }
        
        var token =_http.Login(u, p);
        
        _rt.ConnectToHub(token);  
        
        var user = _http.GetMe();
        
        var user_local = _db.Users.Save(new User
        {
            Id = user.Id,
            UniqueName = user.UniqueName,
            DisplayName = user.DisplayName
        });

        return user_local;
    }
    
    public CurrentUser UpdateMeDisplayName(Guid id, string displayName)
    {
        var user = _db.CurrentUser.Get();
        var user_serv = _http.UpdateMeDisplayName(displayName);

        if (user != null)
        {
            user.DisplayedName = displayName;
            _db.CurrentUser.Save(user);
        }

        return user!;
    }
    
    public CurrentUser GetCurrentUser()
    {
        var user = _db.CurrentUser.Get();
        return user;
    }
    
    public User GetUserByNameWithServer(string uniqueName)
    {
        User b = _db.Users.FindByUniqueName(uniqueName);
        if (b == null)
        {
            b = _http.GetUserByName(uniqueName);
            _db.Users.Save(b);
        }
        return b;
    }

    public User? GetUserById(Guid id)
        => _db.Users.Find(id);

    public User UpdateContactName(Guid id, string contact)
    {
        var user = _db.Users.Find(id);
        User a = _http.UpdateContactName(id, contact);

        if (user != null)
        {
            user.ContactName = contact;
            _db.Users.Save(user);
        }

        return user!;
    }

    public List<User> FindUsersWithContactName()
    {
        return _db.Users.FindUsersWithContactName();

    }

    // ================= USERS =================

    public User FindUsersByUniqueName(string uniqueName)
        => _db.Users.FindByUniqueName(uniqueName);

    // ================= CHATS =================

    public List<Chat> GetAllChats()
        => _db.Chats.GetAllChats();

    public Chat CreateGroupChat(string name, Guid creatorId, List<Guid> participants)
    {
        var chat_http = _http.CreateGroupChat(name, participants);
        
        var chat = new Chat
        {
            Id = chat_http.Id,
            OwnerId = chat_http.OwnerId,
            Name =chat_http.Name,
            CreatedAt = chat_http.CreatedAt,
            UpdatedAt = chat_http.UpdatedAt,
            Version = chat_http.Version,
            Type = chat_http.Type,
            LastMessageNum = chat_http.LastMessageNum,
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
        var chat_http = _http.CreatePrivateChat(participants.Last());
        
        var chat = new Chat
        {
            Id = chat_http.Id,
            OwnerId = chat_http.OwnerId,
            Name =chat_http.Name,
            CreatedAt = chat_http.CreatedAt,
            UpdatedAt = chat_http.UpdatedAt,
            Version = chat_http.Version,
            Type = chat_http.Type,
            LastMessageNum = chat_http.LastMessageNum,
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
        if (user == null) return;

        _db.Chats.AddUserToChat(chatId, user.Id);
    }

    public void LeaveChat(Guid chatId, Guid userId)
    {
        SyncChatResult a = _http.RemoveUserFromChat(chatId, userId);
        _db.Chats.RemoveUserFromChat(chatId, userId);
    }
    
    // ================= MESSAGES =================

    public List<Message> GetChatMessages(Guid chatId)
    {
        var list = _db.Messages.FindChatMessages(chatId);
        _db.Chats.UpdateLastMessageNum(chatId, list.Last().MessageNumber);
        return list;
    }

    public Message SendMessage(Guid chatId, Guid senderId, string text)
    {
        // 1. берём чат
        var chat = _db.Chats.Find(chatId)
                   ?? throw new Exception("Chat not found");

        // 2. отправляем  получаем sync
        var sync = _http.SendMessage(chatId, text, chat.Version);

        if (sync.Messages == null || sync.Messages.Count == 0)
            throw new Exception("No messages returned from server");

        // 3. сохраняем ВСЕ сообщения (без дублей)
        foreach (var m in sync.Messages)
        {
            var exists = _db.Messages.Find(m.MessageNumber);
            if (exists == null)
            {
                _db.Messages.Save(m);
            }
        }
        // 4. обновляем чат ПОЛНОСТЬЮ
        chat.Version = sync.LastVersion;

        var lastMsg = sync.Messages
            .OrderBy(m => m.MessageNumber)
            .Last();

        chat.LastMessageNum = lastMsg.MessageNumber;

        _db.Chats.Save(chat);

        // 5. возвращаем последнее сообщение (обычно твоё)
        return lastMsg;
    }

    // public Message EditMessage(long messageId, string newText)
    // {
    //     var msg = _db.Messages.Edit(messageId, DateTime.UtcNow, newText);
    //
    //     Events.RaiseMessageUpdated(msg);
    //
    //     return msg;
    // }
    //
    // public void DeleteMessage(long messageId)
    // {
    //     _db.Messages.Delete(messageId);
    //
    //
    //
    //     Events.RaiseMessageDeleted(messageId);
    // }
    //
    // // ================= RT =================
    //
    // private void OnMessageReceived(Message m)
    // {
    //     var exists = _db.Messages.Find(m.MessageNumber);
    //     if (exists != null)
    //         return;
    //
    //     _db.Messages.Save(m);
    //     Events.RaiseMessageReceived(m);
    // }
    //
    // private void OnMessageUpdated(Message m)
    // {
    //     _db.Messages.Edit(m.MessageNumber, DateTime.UtcNow, m.Text);
    //     Events.RaiseMessageUpdated(m);
    // }
    //
    // private void OnMessageDeleted(long id)
    // {
    //     _db.Messages.Delete(id);
    //     Events.RaiseMessageDeleted(id);
    // }

    public void UpdateLastReadMessageNum(Guid chatId, Guid userId)
    {
        var messages = GetChatMessages(chatId);
        _db.Chats.UpdateLastReadMessageNum(chatId, userId, messages.Last().MessageNumber);
    }
}