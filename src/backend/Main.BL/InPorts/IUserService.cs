using Main.BL.Models;
namespace Main.BL.InPorts;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid userId, Guid currentUserId);
    Task<User> GetMyProfileAsync(Guid currentUserId);
    Task<IEnumerable<User>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId);
}
