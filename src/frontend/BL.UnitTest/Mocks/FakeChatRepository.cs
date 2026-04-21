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

    public Chat Find(Guid id)
    {
        MaybeThrow();
        return _chats.TryGetValue(id, out var c) ? c : null;
    }

    public List<Chat> GetAllChats()
    {
        MaybeThrow();
        return _chats.Values.ToList();
    }

    public Chat Save(Chat chat)
    {
        MaybeThrow();
        _chats[chat.Id] = chat;
        return chat;
    }

    public void AddUserToChat(Guid chatId, Guid userId)
    {
        MaybeThrow();

        if (!_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId] = new List<Guid>();

        if (!_chatUsers[chatId].Contains(userId))
            _chatUsers[chatId].Add(userId);
    }

    public void RemoveUserFromChat(Guid chatId, Guid userId)
    {
        MaybeThrow();

        if (_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId].Remove(userId);
    }

    public List<User> FindChatUsers(Guid chatId)
    {
        MaybeThrow();

        if (!_chatUsers.ContainsKey(chatId))
            return new List<User>();

        return _chatUsers[chatId]
            .Select(id => new User { Id = id })
            .ToList();
    }

    // ================= EXTRA =================

    public void Delete(Guid id)
    {
        MaybeThrow();

        _chats.Remove(id);
        _chatUsers.Remove(id);
    }

    public Chat UpdateName(Guid chatId, string name)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Name = name;
            return chat;
        }

        return null;
    }

    public Chat UpdateVersion(Guid chatId, long version)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Version = (ulong)version;
            return chat;
        }

        return null;
    }

    public Chat UpdateLastMessageNum(Guid chatId, ulong lastMessageNum)
    {
        MaybeThrow();

        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.LastMessageNum = lastMessageNum;
            return chat;
        }

        return null;
    }

    public void UpdateLastReadMessageNum(Guid chatId, Guid userId, ulong lastReadMessageNum)
    {
        MaybeThrow();
        // можно оставить пустым
    }
}