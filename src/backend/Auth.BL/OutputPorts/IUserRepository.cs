using Auth.BL.Models;
namespace Auth.BL.OutputPorts;

public interface IUserRepository
{
    Task<User?> FindUserByNameAsync(string uniqueName);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
}
