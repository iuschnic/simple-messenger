namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeChatRepository : IChatRepository
{
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, List<Guid>> _chatUsers = new();

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public Task<Chat?> Find(Guid id)
    {
        MaybeThrow();
        _chats.TryGetValue(id, out var chat);
        return Task.FromResult(chat);
    }

    public Task<List<Chat>> GetAllChats()
    {
        MaybeThrow();
        return Task.FromResult(_chats.Values.ToList());
    }

    public Task<Chat> Save(Chat chat)
    {
        MaybeThrow();
        _chats[chat.Id] = chat;
        return Task.FromResult(chat);
    }

    public Task AddUserToChat(Guid chatId, Guid userId)
    {
        MaybeThrow();

        if (!_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId] = new List<Guid>();

        if (!_chatUsers[chatId].Contains(userId))
            _chatUsers[chatId].Add(userId);

        return Task.CompletedTask;
    }

    public Task RemoveUserFromChat(Guid chatId, Guid userId)
    {
        MaybeThrow();

        if (_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId].Remove(userId);

        return Task.CompletedTask;
    }

    public Task<List<User>> FindChatUsers(Guid chatId)
    {
        MaybeThrow();

        if (!_chatUsers.ContainsKey(chatId))
            return Task.FromResult(new List<User>());

        var users = _chatUsers[chatId]
            .Select(id => new User { Id = id })
            .ToList();

        return Task.FromResult(users);
    }

    // ================= EXTRA =================

    public Task Delete(Guid id)
    {
        MaybeThrow();

        _chats.Remove(id);
        _chatUsers.Remove(id);

        return Task.CompletedTask;
    }

    public Task<Chat?> UpdateName(Guid chatId, string name)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Name = name;
            return Task.FromResult<Chat?>(chat);
        }

        return Task.FromResult<Chat?>(null);
    }

    public Task<Chat?> UpdateVersion(Guid chatId, long version)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Version = (ulong)version;
            return Task.FromResult<Chat?>(chat);
        }

        return Task.FromResult<Chat?>(null);
    }

    public Task<Chat?> UpdateLastMessageNum(Guid chatId, ulong lastMessageNum)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.LastMessageNum = lastMessageNum;
            return Task.FromResult<Chat?>(chat);
        }

        return Task.FromResult<Chat?>(null);
    }

    public Task UpdateLastReadMessageNum(Guid chatId, Guid userId, ulong lastReadMessageNum)
    {
        MaybeThrow();
        return Task.CompletedTask;
    }
    public Task LeaveAndDeleteChat(Guid chatId, Guid userId)
    {
        MaybeThrow();

        // 1. Удаляем пользователя из чата
        if (_chatUsers.ContainsKey(chatId))
        {
            _chatUsers[chatId].Remove(userId);

            // 2. Удаляем ВСЕ связи пользователей (эмуляция удаления чата)
            _chatUsers.Remove(chatId);
        }

        // 3. Удаляем сам чат
        _chats.Remove(chatId);

        return Task.CompletedTask;
    }
}