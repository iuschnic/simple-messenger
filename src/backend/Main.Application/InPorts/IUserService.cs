using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId, Guid currentUserId);
    Task<UserDto> GetMyProfileAsync(Guid currentUserId);
    Task<UserDto> CreateUserAsync(string uniqueName, string displayedName);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId);
}
