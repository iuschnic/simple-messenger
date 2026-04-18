using System.Net;
using BL.Contracts;
using BL.Models;

namespace Messenger.Tests.Sandbox.Mocks;

public class FakeHttpClient : IHttpClient
{
    private readonly Dictionary<long, Message> _messages = new();
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<Guid, CurrentUser> _currentuser = new();

    private long _msgCounter = 1;
    private long _version = 1;
    

    // ================= AUTH =================

    public HttpResponseMessage Register(string uniqueName, string password, string email, string displayName)
    {
        // проверка дубля (как сервер)
        if (_users.Values.Any(u => u.UniqueName == uniqueName))
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = displayName
        };

        _users[user.Id] = user;

        return new HttpResponseMessage(HttpStatusCode.Created);
    }

    public string Login(string uniqueName, string password)
    {
        return "fake-token";
    }

    private static readonly Guid TestUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public User GetMe()
    {
        return new User
        {
            Id = TestUserId,
            UniqueName = "alice",
            DisplayName = "yxye"
        };
    }

    // ================= USERS =================

    public User GetUser(Guid id)
        => _users.TryGetValue(id, out var user)
            ? user
            : new User { Id = id, UniqueName = "unknown" };
    
    public User GetUserByName(string uniqueName)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = "name"
        };
    }
    
    public SyncChatResult RemoveUserFromChat(Guid chatId, Guid userId)
    {
        return new SyncChatResult
        {
            ChatId = Guid.NewGuid(),
            Messages = new List<Message>(),
            LastVersion = 0
        };
    }

    private static readonly Guid TestChatId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Chat CreateGroupChat(string name, List<string> memberIds)
    {
        return new Chat
        {
            Id = TestChatId,
            OwnerId = GetMe().Id,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1,
            Type = "group",
            LastMessageNum = 0
        };;
    }
    
    private static readonly Guid TestPrivateChatId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Chat CreatePrivateChat(String withUserId)
    {
        return new Chat
        {
            Id = TestPrivateChatId,
            OwnerId = GetMe().Id,
            Name = GetUser(Guid.Parse(withUserId)).UniqueName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1,
            Type = "private",
            LastMessageNum = 0
        };;
    }
    
    
    public CurrentUser UpdateMeDisplayName(string displayName)
    {
        return null;
    }

    public User UpdateContactName(Guid id, string contactName)
    {
        return null;
    }

    public List<User> SearchUsers(string substr, int maxUsers)
    {
        return _users.Values
            .Where(u => u.UniqueName.Contains(substr ?? "", StringComparison.OrdinalIgnoreCase))
            .Take(maxUsers)
            .ToList();
    }

    // ================= CHATS =================

    public List<Chat> GetChats()
        => _chats.Values.ToList();

    public Chat CreateGroupChat(string name, List<Guid> memberIds)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = memberIds.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow,
            Type = "group",
            Version = _version++
        };

        _chats[chat.Id] = chat;
        return chat;
    }

    public Chat CreatePrivateChat(Guid withUserId)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = "private",
            OwnerId = withUserId,
            CreatedAt = DateTime.UtcNow,
            Type = "private",
            Version = _version++
        };

        _chats[chat.Id] = chat;
        return chat;
    }

    // ================= MESSAGES =================

    public SyncChatResult SendMessage(Guid chatId, string text, long clientVersion)
    {
        var msg = new Message
        {
            MessageNumber = _msgCounter++,
            ChatId = chatId,
            SenderId = GetMe().Id,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Version = _version++,
            Type = MessageType.Regular
        };

        _messages[msg.MessageNumber] = msg;

        return new SyncChatResult
        {
            Messages = new List<Message> { msg },
            LastVersion = msg.Version
        };
    }

    public SyncChatResult EditMessage(Guid chatId, long messageNum, string newText, long clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var msg))
        {
            msg.Text = newText;
            msg.EditedAt = DateTime.UtcNow;
            msg.Version = _version++;
        }

        return new SyncChatResult
        {
            Messages = new List<Message> { msg },
            LastVersion = _version
        };
    }

    public SyncChatResult DeleteMessage(Guid chatId, long messageNum, long clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var msg))
        {
            _messages.Remove(messageNum);

            msg.Deleted = true;
            msg.Version = _version++;

            return new SyncChatResult
            {
                Messages = new List<Message> { msg },
                LastVersion = _version
            };
        }

        return new SyncChatResult
        {
            Messages = new List<Message>(),
            LastVersion = _version
        };
    }

    public List<Message> GetMessages(Guid chatId, long? fromMessageNumber = null, int? limit = null)
    {
        IEnumerable<Message> query = _messages.Values
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.MessageNumber);

        if (fromMessageNumber != null)
            query = query.Where(m => m.MessageNumber >= fromMessageNumber.Value);

        if (limit != null)
            query = query.Take(limit.Value);

        return query.ToList();
    }
    
    public Chat GetChat(Guid chatId)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = "private",
            OwnerId = new Guid(),
            CreatedAt = DateTime.UtcNow,
            Type = "private",
            Version = _version++
        };

        _chats[chat.Id] = chat;
        return chat;
    }
    
    public List<SyncChatResult> SyncChats(List<(Guid chatId, long version)> chats)
    {
        return new List<SyncChatResult>
        {
            new SyncChatResult
            {
                ChatId = chats.First().chatId, // не забудь!
                Messages = new List<Message>(),
                LastVersion = _version
            }
        };
    }
}