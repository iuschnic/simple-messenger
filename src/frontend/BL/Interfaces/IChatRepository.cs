using BL.Models;

namespace BL.Interfaces;

public interface IChatRepository
{
    Chat Find(Guid id);
    public List<Chat> GetAllChats();

    List<User> FindChatUsers(Guid chatId);

    void AddUserToChat(Guid chatId, Guid userId);
    void RemoveUserFromChat(Guid chatId, Guid userId);

    Chat Save(Chat chat);
    Chat UpdateName(Guid chatId, string name);
    Chat UpdateVersion(Guid chatId, long version);

    void Delete(Guid id);
    public Chat UpdateLastMessageNum(Guid chatId, long lastMessageNum);
    public void UpdateLastReadMessageNum(Guid chatId, Guid userId, long lastReadMessageNum);
}