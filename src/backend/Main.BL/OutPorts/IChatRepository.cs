using Main.BL.Models;

namespace Main.BL.OutPorts;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id);
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId);
    Task<Chat> CreateAsync(Chat chat);
    Task UpdateNameAsync(Guid chatId, string newName);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
