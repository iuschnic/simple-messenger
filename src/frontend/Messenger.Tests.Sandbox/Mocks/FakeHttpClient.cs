using BL.Contracts;
using BL.Models;

namespace Messenger.Tests.Sandbox.Mocks;

public class FakeHttpClient : IHttpClient
{
    private readonly Dictionary<ulong, Message> _messages = new();
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, User> _users = new();

    private ulong _msgCounter = 1;
    private ulong _version = 1;

    private static readonly Guid TestUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    // ================= AUTH =================

    public Task Register(string uniqueName, string password, string email, string displayName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = displayName
        };

        _users[user.Id] = user;

        return Task.CompletedTask;
    }

    public Task<string> Login(string uniqueName, string password)
    {
        return Task.FromResult("fake-token");
    }

    public Task<User> GetMe()
    {
        return Task.FromResult(new User
        {
            Id = TestUserId,
            UniqueName = "alice",
            DisplayName = "yxye"
        });
    }

    public Task<User> GetUser(Guid id)
    {
        var user = _users.TryGetValue(id, out var u)
            ? u
            : new User { Id = id, UniqueName = "unknown" };

        return Task.FromResult(user);
    }

    public Task<User> GetUserByName(string uniqueName)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.UniqueName.Equals(uniqueName, StringComparison.OrdinalIgnoreCase));

        user ??= new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = "name"
        };

        return Task.FromResult(user);
    }

    public Task<CurrentUser> UpdateMeDisplayName(string displayName)
    {
        return Task.FromResult(new CurrentUser
        {
            Id = TestUserId,
            DisplayedName = displayName
        });
    }

    public Task<User> UpdateContactName(Guid id, string contactName)
    {
        var user = _users.GetValueOrDefault(id) ?? new User { Id = id };

        user.DisplayName = contactName;
        _users[id] = user;

        return Task.FromResult(user);
    }

    public Task<List<User>> SearchUsers(string substr, int maxUsers)
    {
        var list = _users.Values
            .Where(u => u.UniqueName.Contains(substr ?? "", StringComparison.OrdinalIgnoreCase))
            .Take(maxUsers)
            .ToList();

        return Task.FromResult(list);
    }

    // ================= CHATS =================

    public Task<List<Chat>> GetChats()
    {
        return Task.FromResult(_chats.Values.ToList());
    }

    public Task<Chat> CreateGroupChat(string name, List<Guid> memberIds)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = memberIds.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow,
            Type = ChatType.Group,
            Version = _version++
        };

        _chats[chat.Id] = chat;

        return Task.FromResult(chat);
    }

    public Task<Chat> CreatePrivateChat(Guid withUserId)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = "private",
            OwnerId = withUserId,
            CreatedAt = DateTime.UtcNow,
            Type = ChatType.Private,
            Version = _version++
        };

        _chats[chat.Id] = chat;

        return Task.FromResult(chat);
    }

    public Task<Chat> GetChat(Guid chatId)
    {
        if (_chats.TryGetValue(chatId, out var chat))
            return Task.FromResult(chat);

        var newChat = new Chat
        {
            Id = chatId,
            Name = "private",
            OwnerId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            Type = ChatType.Private,
            Version = _version++
        };

        _chats[chatId] = newChat;

        return Task.FromResult(newChat);
    }

    public Task<List<SyncChatResult>> SyncChats(List<(Guid chatId, ulong version)> chats)
    {
        var result = chats.Select(c => new SyncChatResult
        {
            ChatId = c.chatId,
            Messages = new List<Message>(),
            LastVersion = _version
        }).ToList();

        return Task.FromResult(result);
    }

    public Task<SyncChatResult> RemoveUserFromChat(Guid chatId, Guid userId, ulong version)
    {
        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<Message>(),
            LastVersion = _version
        });
    }

    // ================= MESSAGES =================

    public Task<SyncChatResult> SendMessage(Guid chatId, string text, ulong clientVersion)
    {
        var msg = new Message
        {
            MessageNumber = _msgCounter++,
            ChatId = chatId,
            SenderId = TestUserId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Version = _version++,
            Type = MessageType.Regular
        };

        _messages[msg.MessageNumber] = msg;

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<Message> { msg },
            LastVersion = msg.Version
        });
    }

    public Task<SyncChatResult> EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var msg))
        {
            msg.Text = newText;
            msg.EditedAt = DateTime.UtcNow;
            msg.Version = _version++;
        }

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = msg != null ? new List<Message> { msg } : new List<Message>(),
            LastVersion = _version
        });
    }

    public Task<SyncChatResult> DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var msg))
        {
            _messages.Remove(messageNum);

            msg.Deleted = true;
            msg.Version = _version++;

            return Task.FromResult(new SyncChatResult
            {
                ChatId = chatId,
                Messages = new List<Message> { msg },
                LastVersion = _version
            });
        }

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<Message>(),
            LastVersion = _version
        });
    }

    public Task<List<Message>> GetMessages(Guid chatId, ulong? fromMessageNumber = null, int? limit = null)
    {
        IEnumerable<Message> query = _messages.Values
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.MessageNumber);

        if (fromMessageNumber != null)
            query = query.Where(m => m.MessageNumber >= fromMessageNumber.Value);

        if (limit != null)
            query = query.Take(limit.Value);

        return Task.FromResult(query.ToList());
    }
}