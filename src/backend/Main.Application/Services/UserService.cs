using Main.Application.InPorts;
using Main.BL.Models;
using Main.Application.OutPorts;
using Main.BL.Exceptions;

namespace Main.Application.Services;

public class UserService : BaseService, IUserService
{
    public UserService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo) : base(userRepo, chatRepo, chatUserRepo, messageRepo) { }

    public async Task<User> GetUserByIdAsync(Guid userId, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        return await GetUserOrThrow(userId);
    }
    public async Task<User> CreateUserAsync(string uniqueName, string displayedName)
    {
        var user = User.CreateNew(uniqueName, displayedName);
        if (!await _userRepo.CreateAsync(user))
            throw new TechnicalException("Failed to create user");
        return user;
    }
    public async Task<User> GetMyProfileAsync(Guid currentUserId)
    {
        return await GetUserOrThrow(currentUserId);
    }
    public async Task<IEnumerable<User>> SearchUsersAsync(string substr, int maxUsers, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var users = await _userRepo.SearchAsync(substr, maxUsers);
        return users.Where(u => u.Id != currentUserId);
    }
}
