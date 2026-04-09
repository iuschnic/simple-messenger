using Main.BL.Models;

namespace Main.BL.OutPorts;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id);
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId);
    Task CreateAsync(Chat chat);
    Task<bool> TryUpdateNameAsync(Guid chatId, string newName);
    Task<bool> TryDeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsPrivateBetweenUsersAsync(Guid user1Id, Guid user2Id);
}
