namespace BL.UnitTest.Mocks;

using BL.Contracts;
using BL.Models;
using BL.Exceptions;

public class FakeHttpClient : IHttpClient
{
    private readonly Dictionary<ulong, Message> _messages = new();
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, User> _users = new();

    private ulong _msgCounter = 1;
    private ulong _version = 1;

    public Exception? ExceptionToThrow { get; set; }

    // ================= ERROR CORE =================

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
        {
            var ex = ExceptionToThrow;
            ExceptionToThrow = null;
            throw ex;
        }
    }

    private Task<T> Wrap<T>(Func<T> func)
    {
        MaybeThrow();
        return Task.FromResult(func());
    }

    private Task Wrap(Action action)
    {
        MaybeThrow();
        action();
        return Task.CompletedTask;
    }

    // ================= AUTH =================

    public Task Register(string uniqueName, string password, string email, string displayName)
        => Wrap(() =>
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UniqueName = uniqueName,
                DisplayName = displayName
            };

            _users[user.Id] = user;
        });

    public Task<string> Login(string uniqueName, string password)
        => Wrap(() => "fake-token");

    private static readonly Guid TestUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Task<User> GetMe()
        => Wrap(() => new User
        {
            Id = TestUserId,
            UniqueName = "alice",
            DisplayName = "yxye"
        });

    // ================= USERS =================

    public Task<User> GetUser(Guid id)
        => Wrap(() =>
            _users.TryGetValue(id, out var user)
                ? user
                : new User { Id = id, UniqueName = "unknown" });

    public Task<User> GetUserByName(string uniqueName)
        => Wrap(() => new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = "name"
        });

    public Task<CurrentUser> UpdateMeDisplayName(string displayName)
        => Wrap(() => new CurrentUser
        {
            Id = TestUserId,
            DisplayedName = displayName
        });

    public Task<User> UpdateContactName(Guid id, string contactName)
        => Wrap(() => new User
        {
            Id = id,
            ContactName = contactName
        });

    public Task<List<User>> SearchUsers(string substr, int maxUsers)
        => Wrap(() =>
            _users.Values
                .Where(u => u.UniqueName.Contains(substr ?? "", StringComparison.OrdinalIgnoreCase))
                .Take(maxUsers)
                .ToList());

    // ================= CHATS =================

    public Task<List<Chat>> GetChats()
        => Wrap(() => _chats.Values.ToList());

    public Task<Chat> CreateGroupChat(string name, List<Guid> memberIds)
        => Wrap(() =>
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
            return chat;
        });

    public Task<Chat> CreatePrivateChat(Guid withUserId)
        => Wrap(() =>
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
            return chat;
        });

    public Task<Chat> GetChat(Guid chatId)
        => Wrap(() =>
        {
            if (_chats.TryGetValue(chatId, out var chat))
                return chat;

            var newChat = new Chat
            {
                Id = chatId,
                Name = "chat",
                CreatedAt = DateTime.UtcNow,
                Version = _version++
            };

            _chats[chatId] = newChat;
            return newChat;
        });

    public Task<SyncChatResult> RemoveUserFromChat(Guid chatId, Guid userId)
        => Wrap(() => new SyncChatResult
        {
            ChatId = chatId,
            Messages = new List<Message>(),
            LastVersion = _version++
        });

    public Task<List<SyncChatResult>> SyncChats(List<(Guid chatId, ulong version)> chats)
        => Wrap(() =>
        {
            var chatId = chats.First().chatId;

            var msgs = _messages.Values
                .Where(m => m.ChatId == chatId)
                .ToList();

            return new List<SyncChatResult>
            {
                new SyncChatResult
                {
                    ChatId = chatId,
                    Messages = msgs,
                    LastVersion = _version
                }
            };
        });

    // ================= MESSAGES =================

    public Task<SyncChatResult> SendMessage(Guid chatId, string text, ulong clientVersion)
        => Wrap(() =>
        {
            var newVersion = ++_version;

            var msg = new Message
            {
                MessageNumber = _msgCounter++,
                ChatId = chatId,
                SenderId = TestUserId,
                Text = text,
                CreatedAt = DateTime.UtcNow,
                Version = newVersion,
                Type = MessageType.Regular
            };

            _messages[msg.MessageNumber] = msg;

            return new SyncChatResult
            {
                Messages = new List<Message> { msg },
                LastVersion = newVersion
            };
        });

    public Task<SyncChatResult> EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
        => Wrap(() =>
        {
            _messages.TryGetValue(messageNum, out var msg);

            if (msg != null)
            {
                msg.Text = newText;
                msg.EditedAt = DateTime.UtcNow;
                msg.Version = _version++;
            }

            return new SyncChatResult
            {
                Messages = msg != null ? new List<Message> { msg } : new List<Message>(),
                LastVersion = _version
            };
        });

    public Task<SyncChatResult> DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
        => Wrap(() =>
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
        });

    public Task<List<Message>> GetMessages(Guid chatId, ulong? fromMessageNumber = null, int? limit = null)
        => Wrap(() =>
        {
            IEnumerable<Message> query = _messages.Values
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.MessageNumber);

            if (fromMessageNumber != null)
                query = query.Where(m => m.MessageNumber >= fromMessageNumber.Value);

            if (limit != null)
                query = query.Take(limit.Value);

            return query.ToList();
        });
}