using Main.Application.InPorts;
using Main.BL.Models;
using Main.Application.OutPorts;
using Main.Application.Exceptions;
using Main.Application.Dtos;
using Main.Application.Mappers;

namespace Main.Application.Services;

public class UserService : BaseService, IUserService
{
    public UserService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo,
        IMessageProducer messageProducer) : base(userRepo, chatRepo, chatUserRepo, messageRepo, messageProducer) { }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        var user = await GetUserOrNotFound(userId);
        return user.ToDto();
    }
    public async Task<UserDto> CreateUserAsync(Guid id, string uniqueName, string displayedName)
    {
        if (string.IsNullOrWhiteSpace(displayedName))
            throw new ArgumentException("Invalid displayed name");
        if (string.IsNullOrWhiteSpace(uniqueName))
            throw new ArgumentException("Invalid unique name");
        if (await _userRepo.ExistsByUniqueNameAsync(uniqueName))
            throw new RuleViolationException("User with the same unique name already exists");
        var user = User.Create(id, uniqueName, displayedName);
        if (!await _userRepo.CreateAsync(user))
            throw new TechnicalException("Failed to create user");
        return user.ToDto();
    }
    public async Task RemoveUserAsync(Guid id)
    {
        await EnsureUserExists(id);
        if (!await _userRepo.DeleteAsync(id))
            throw new TechnicalException("Failed to remove user");
    }
    public async Task<UserDto> UpdateDisplayedNameAsync(string newDisplayedName, Guid currentUserId)
    {
        if (string.IsNullOrWhiteSpace(newDisplayedName))
            throw new ArgumentException("Invalid displayed name");
        var user = await GetCurrentUserOrUnauthorized(currentUserId);
        if (!await _userRepo.UpdateDisplayedNameAsync(currentUserId, newDisplayedName))
            throw new TechnicalException("Failed to update user displayed name");

        await _messageProducer.SendUserChangedAsync(user.Id, user.UniqueName, newDisplayedName);

        user = User.Create(user.Id, user.UniqueName, newDisplayedName);
        return user.ToDto();
    }
    public async Task<UserDto> GetMyProfileAsync(Guid currentUserId)
    {
        var user = await GetCurrentUserOrUnauthorized(currentUserId);
        return user.ToDto();
    }
    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId)
    {
        if (string.IsNullOrWhiteSpace(substr))
            throw new ArgumentException("Invalid displayed name");
        if (maxUsers <= 0)
            throw new ArgumentException("Invalid maxUsers");
        await EnsureCurrentUserAuthorized(currentUserId);
        var users = await _userRepo.SearchAsync(substr, maxUsers);
        return users.Where(u => u.Id != currentUserId).Select(u => u.ToDto());
    }
}
