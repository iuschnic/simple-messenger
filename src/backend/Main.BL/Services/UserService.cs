using Main.BL.InPorts;
using Main.BL.Models;
using Main.BL.OutPorts;

namespace Main.BL.Services;

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
