using Main.BL.Models;

namespace Main.BL.OutPorts;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUniqueNameAsync(string uniqueName);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByUniqueNameAsync(string uniqueName);
    Task<bool> CreateAsync(User user);
    Task<bool> UpdateDisplayedNameAsync(Guid id, string newDisplayedName);
    Task<bool> DeleteAsync(Guid id);
}