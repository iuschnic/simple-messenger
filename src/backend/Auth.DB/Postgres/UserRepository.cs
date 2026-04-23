using Auth.BL.Models;
using Auth.BL.OutputPorts;
using Microsoft.EntityFrameworkCore;

namespace Auth.DB.Postgres;
public class UserRepository : IUserRepository
{
    private readonly ServerDbContext _dbContext;

    public UserRepository(ServerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> FindUserByNameAsync(string uniqueName)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UniqueName == uniqueName)
            .ConfigureAwait(false);
    }

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
