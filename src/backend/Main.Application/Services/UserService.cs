using Main.Application.InPorts;
using Main.BL.Models;
using Main.Application.OutPorts;
using Main.BL.Exceptions;
using Main.Application.Dtos;
using Main.Application.Mappers;

namespace Main.Application.Services;

public class UserService : BaseService, IUserService
{
    public UserService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo) : base(userRepo, chatRepo, chatUserRepo, messageRepo) { }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var user = await GetUserOrThrow(userId);
        return user.ToDto();
    }
    public async Task<UserDto> CreateUserAsync(string uniqueName, string displayedName)
    {
        if (await _userRepo.ExistsByUniqueNameAsync(uniqueName))
            throw new RuleViolationException("User with the same unique name already exists");
        var user = User.CreateNew(uniqueName, displayedName);
        if (!await _userRepo.CreateAsync(user))
            throw new TechnicalException("Failed to create user");
        return user.ToDto();
    }
    public async Task<UserDto> GetMyProfileAsync(Guid currentUserId)
    {
        var user = await GetUserOrThrow(currentUserId);
        return user.ToDto();
    }
    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var users = await _userRepo.SearchAsync(substr, maxUsers);
        return users.Where(u => u.Id != currentUserId).Select(u => u.ToDto());
    }
}
