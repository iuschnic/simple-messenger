using BL.Models;

namespace BL.Interfaces;

public interface IChatRepository
{
    Task<Chat?> Find(Guid id);
    Task<List<Chat>> GetAllChats();

    Task<List<User>> FindChatUsers(Guid chatId);

    Task AddUserToChat(Guid chatId, Guid userId);
    Task RemoveUserFromChat(Guid chatId, Guid userId);

    Task<Chat> Save(Chat chat);
    Task<Chat?> UpdateName(Guid chatId, string name);
    Task<Chat?> UpdateVersion(Guid chatId, long version);

    Task Delete(Guid id);

    Task<Chat?> UpdateLastMessageNum(Guid chatId, ulong lastMessageNum);
    Task UpdateLastReadMessageNum(Guid chatId, Guid userId, ulong lastReadMessageNum);
    Task LeaveAndDeleteChat(Guid chatId, Guid userId);
}