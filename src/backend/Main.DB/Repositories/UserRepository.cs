using Main.BL.Models;
using Main.Application.OutPorts;
using Main.Application.Exceptions;
using Main.DB.Converters;
using Main.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MainDbContext _context;

    public UserRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var userDb = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return userDb?.ToDomain();
    }
    public async Task<User?> GetByUniqueNameAsync(string uniqueName)
    {
        var userDb = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UniqueName == uniqueName);
        return userDb?.ToDomain();
    }
    public async Task<IEnumerable<User>> GetByIdsAsync(List<Guid> userIds)
    {
        var usersDb = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
        return usersDb.Select(u => u.ToDomain());
    }
    public async Task<IEnumerable<User>> SearchAsync(
        string substr,
        int maxUsers,
        Guid excludeUserId)
    {
        var query = _context.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(substr))
        {
            query = query.Where(u =>
                u.Id != excludeUserId &&
                (u.UniqueName.Contains(substr) ||
                u.DisplayedName.Contains(substr)));
        }
        var usersDb = await query
            .OrderBy(u => u.DisplayedName)
            .Take(maxUsers)
            .ToListAsync();
        return usersDb.Select(u => u.ToDomain());
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }
    public async Task<bool> ExistsByUniqueNameAsync(string uniqueName)
    {
        return await _context.Users.AnyAsync(u => u.UniqueName == uniqueName);
    }
    public async Task CreateAsync(User user)
    {
        if (await _context.Users.AnyAsync(u => u.UniqueName == user.UniqueName || u.Id == user.Id))
            throw new ConflictException($"User with unique name {user.UniqueName}already exists");
        var userDb = user.ToDb();
        await _context.Users.AddAsync(userDb);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(User user)
    {
        var userDb = await _context.Users.FindAsync(user.Id)
            ?? throw new NotFoundException($"User {user.Id} not found");

        userDb.DisplayedName = user.DisplayedName;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Guid id)
    {
        var userDb = await _context.Users.FindAsync(id)
            ?? throw new NotFoundException($"User {id} not found");

        _context.Users.Remove(userDb);
        await _context.SaveChangesAsync();
    }
}