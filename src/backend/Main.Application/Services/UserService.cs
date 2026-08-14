using Main.Application.InPorts;
using Main.BL.Models;
using Main.Application.OutPorts;
using Main.Application.Exceptions;
using Main.Application.Dtos;
using Main.Application.Mappers;

namespace Main.Application.Services;

public static class UserPaging
{
    public const int MaxPageSize = 10;
    public const int MinPageSize = 1;
}

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
        if (await _userRepo.ExistsByUniqueNameAsync(uniqueName))
            throw new ConflictException("User with the same unique name already exists");

        var user = User.Create(id, uniqueName, displayedName);
        await _userRepo.CreateAsync(user);

        return user.ToDto();
    }
    public async Task RemoveUserAsync(Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await _userRepo.DeleteAsync(currentUserId);
    }
    public async Task<UserDto> UpdateDisplayedNameAsync(string newDisplayedName, Guid currentUserId)
    {
        var user = await GetCurrentUserOrUnauthorized(currentUserId);

        user.ChangeDisplayedName(newDisplayedName);
        await _userRepo.UpdateAsync(user);

        await _messageProducer.SendUserChangedAsync(user.Id, user.UniqueName, user.DisplayedName);

        return user.ToDto();
    }
    public async Task<UserDto> GetMyProfileAsync(Guid currentUserId)
    {
        var user = await GetCurrentUserOrUnauthorized(currentUserId);
        return user.ToDto();
    }
    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId)
    {
        maxUsers = Math.Clamp(maxUsers, UserPaging.MinPageSize, UserPaging.MaxPageSize);

        await EnsureCurrentUserAuthorized(currentUserId);

        if (string.IsNullOrWhiteSpace(substr))
            return [];

        var users = await _userRepo.SearchAsync(substr, maxUsers, currentUserId);
        return users.Select(u => u.ToDto());
    }
}
