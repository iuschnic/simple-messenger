using Main.BL.Models;

namespace Main.Application.OutPorts;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUniqueNameAsync(string uniqueName);
    Task<IEnumerable<User>> GetByIdsAsync(List<Guid> userIds);
    Task<IEnumerable<User>> SearchAsync(string substr, int maxUsers, Guid excludeUserId);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByUniqueNameAsync(string uniqueName);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}