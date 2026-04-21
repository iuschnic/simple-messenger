namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

using BL.Interfaces;
using BL.Models;

public class FakeChatRepository : IChatRepository
{
    private readonly Dictionary<Guid, Chat> _chats = new();
    private readonly Dictionary<Guid, List<Guid>> _chatUsers = new();

    public Chat Find(Guid id)
        => _chats.TryGetValue(id, out var c) ? c : null;

    public List<Chat> GetAllChats()
        => _chats.Values.ToList();

    public Chat Save(Chat chat)
    {
        _chats[chat.Id] = chat;
        return chat;
    }

    public void AddUserToChat(Guid chatId, Guid userId)
    {
        if (!_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId] = new List<Guid>();

        if (!_chatUsers[chatId].Contains(userId))
            _chatUsers[chatId].Add(userId);
    }

    public void RemoveUserFromChat(Guid chatId, Guid userId)
    {
        if (_chatUsers.ContainsKey(chatId))
            _chatUsers[chatId].Remove(userId);
    }

    public List<User> FindChatUsers(Guid chatId)
    {
        if (!_chatUsers.ContainsKey(chatId))
            return new List<User>();

        return _chatUsers[chatId]
            .Select(id => new User { Id = id })
            .ToList();
    }

    // 🔧 ОБЯЗАТЕЛЬНЫЕ методы интерфейса

    public void Delete(Guid id)
    {
        _chats.Remove(id);
        _chatUsers.Remove(id);
    }

    public Chat UpdateName(Guid chatId, string name)
    {
        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Name = name;
        }
        return chat;
    }

    public Chat UpdateVersion(Guid chatId, long version)
    {
        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.Version = (ulong)version;
        }
        return chat;
    }

    public Chat UpdateLastMessageNum(Guid chatId, ulong lastMessageNum)
    {
        if (_chats.TryGetValue(chatId, out var chat))
        {
            chat.LastMessageNum = lastMessageNum;
        }
        return chat;
    }

    public void UpdateLastReadMessageNum(Guid chatId, Guid userId, ulong lastReadMessageNum)
    {
        // можно оставить пустым или добавить хранение при необходимости
    }
}