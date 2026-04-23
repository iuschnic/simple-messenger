using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId, Guid currentUserId);
    Task<UserDto> GetMyProfileAsync(Guid currentUserId);
    Task<UserDto> CreateUserAsync(Guid id, string uniqueName, string displayedName);
    Task<UserDto> UpdateDisplayedNameAsync(string newDisplayedName, Guid currentUserId);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId);
}
