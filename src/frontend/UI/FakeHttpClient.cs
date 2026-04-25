using BL.Contracts;
using BL.Models;
using MessageModel = BL.Models.Message;

namespace UI;

internal sealed class FakeHttpClient : IHttpClient
{
    private readonly Dictionary<ulong, MessageModel> _messages = new();
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, User> _users = new();

    private ulong _messageCounter = 1;
    private ulong _version = 1;

    private static readonly Guid TestUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

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
        => Task.FromResult("fake-token");

    public Task<User> GetMe()
        => Task.FromResult(new User
        {
            Id = TestUserId,
            UniqueName = "alice",
            DisplayName = "yxye"
        });

    public Task<User> GetUser(Guid id)
    {
        var user = _users.TryGetValue(id, out var stored)
            ? stored
            : new User { Id = id, UniqueName = "unknown", DisplayName = "unknown" };

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
            DisplayName = uniqueName
        };

        _users[user.Id] = user;

        return Task.FromResult(user);
    }

    public Task<CurrentUser> UpdateMeDisplayName(string displayName)
        => Task.FromResult(new CurrentUser
        {
            Id = TestUserId,
            DisplayedName = displayName
        });

    public Task<User> UpdateContactName(Guid id, string contactName)
    {
        var user = _users.GetValueOrDefault(id) ?? new User { Id = id, UniqueName = "unknown" };
        user.ContactName = contactName;
        user.DisplayName ??= user.UniqueName;
        _users[id] = user;

        return Task.FromResult(user);
    }

    public Task<List<User>> SearchUsers(string substr, int maxUsers)
    {
        substr ??= string.Empty;

        var result = _users.Values
            .Where(u => u.UniqueName.Contains(substr, StringComparison.OrdinalIgnoreCase))
            .Take(maxUsers)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<Chat>> GetChats()
        => Task.FromResult(_chats.Values.OrderByDescending(c => c.CreatedAt).ToList());

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
            Name = $"private-{withUserId.ToString()[..8]}",
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

        var created = new Chat
        {
            Id = chatId,
            Name = "private",
            OwnerId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            Type = ChatType.Private,
            Version = _version++
        };

        _chats[chatId] = created;

        return Task.FromResult(created);
    }

    public Task<SyncChatResult> SyncChat(Guid chatId, ulong clientVersion)
    {
        var messages = _messages.Values
            .Where(m => m.ChatId == chatId && m.Version > clientVersion)
            .OrderBy(m => m.MessageNumber)
            .ToList();

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = messages,
            Participants = new List<User>(),
            LastVersion = _version
        });
    }

    public Task<List<SyncChatResult>> SyncChats(List<(Guid chatId, ulong version)> chats)
    {
        var result = chats.Select(chat => new SyncChatResult
        {
            ChatId = chat.chatId,
            Messages = _messages.Values
                .Where(m => m.ChatId == chat.chatId && m.Version > chat.version)
                .OrderBy(m => m.MessageNumber)
                .ToList(),
            Participants = new List<User>(),
            LastVersion = _version
        }).ToList();

        return Task.FromResult(result);
    }

    public Task<SyncChatResult> RemoveUserFromChat(Guid chatId, Guid userId, ulong clientVersion)
        => Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<MessageModel>(),
            Participants = new List<User>(),
            LastVersion = _version
        });

    public Task<SyncChatResult> SendMessage(Guid chatId, string text, ulong clientVersion)
    {
        var message = new MessageModel
        {
            MessageNumber = _messageCounter++,
            ChatId = chatId,
            SenderId = TestUserId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Version = _version++,
            Type = MessageType.Regular
        };

        _messages[message.MessageNumber] = message;

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<MessageModel> { message },
            Participants = new List<User>(),
            LastVersion = message.Version
        });
    }

    public Task<SyncChatResult> EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var message))
        {
            message.Text = newText;
            message.EditedAt = DateTime.UtcNow;
            message.Version = _version++;
        }

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = message is null ? new List<MessageModel>() : new List<MessageModel> { message },
            Participants = new List<User>(),
            LastVersion = _version
        });
    }

    public Task<SyncChatResult> DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
    {
        if (_messages.TryGetValue(messageNum, out var message))
        {
            _messages.Remove(messageNum);
            message.Deleted = true;
            message.Version = _version++;

            return Task.FromResult(new SyncChatResult
            {
                ChatId = chatId,
                Messages = new List<MessageModel> { message },
                Participants = new List<User>(),
                LastVersion = _version
            });
        }

        return Task.FromResult(new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<MessageModel>(),
            Participants = new List<User>(),
            LastVersion = _version
        });
    }

    public Task<List<MessageModel>> GetMessages(Guid chatId, ulong fromMessageNumber, int limit)
    {
        var result = _messages.Values
            .Where(m => m.ChatId == chatId && m.MessageNumber >= fromMessageNumber)
            .OrderBy(m => m.MessageNumber)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }
}
