using Main.BL.Models;
namespace Main.Application.InPorts;
public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid userId, Guid currentUserId);
    Task<User> GetMyProfileAsync(Guid currentUserId);
    Task<User> CreateUserAsync(string uniqueName, string displayedName);
    Task<IEnumerable<User>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId);
}
