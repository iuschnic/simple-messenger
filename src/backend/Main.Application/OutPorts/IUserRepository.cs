using Main.BL.Models;

namespace Main.Application.OutPorts;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUniqueNameAsync(string uniqueName);
    Task<IEnumerable<User>> GetByIdsAsync(List<Guid> userIds);
    Task<IEnumerable<User>> SearchAsync(string substr, int maxUsers);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByUniqueNameAsync(string uniqueName);
    Task<bool> CreateAsync(User user);
    Task<bool> UpdateDisplayedNameAsync(Guid id, string newDisplayedName);
    Task<bool> DeleteAsync(Guid id);
}