using Main.BL.Models;
using Main.BL.OutPorts;
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
            .FirstOrDefaultAsync(u => u.Id == id);
        return userDb?.ToDomain();
    }
    public async Task<User?> GetByUniqueNameAsync(string uniqueName)
    {
        var userDb = await _context.Users
            .FirstOrDefaultAsync(u => u.UniqueName == uniqueName);
        return userDb?.ToDomain();
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }
    public async Task<bool> ExistsByUniqueNameAsync(string uniqueName)
    {
        return await _context.Users.AnyAsync(u => u.UniqueName == uniqueName);
    }
    public async Task<bool> CreateAsync(User user)
    {
        var existDb = await _context.Users.FindAsync(user.UniqueName);
        if (await _context.Users.AnyAsync(u => u.UniqueName == user.UniqueName || u.Id == user.Id))
            return false;
        var userDb = user.ToDb();
        await _context.Users.AddAsync(userDb);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateDisplayedNameAsync(Guid id, string newDisplayedName)
    {
        var userDb = await _context.Users.FindAsync(id);
        if (userDb == null)
            return false;
        if (string.IsNullOrWhiteSpace(newDisplayedName))
            return false;
        userDb.DisplayedName = newDisplayedName;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var userDb = await _context.Users.FindAsync(id);
        if (userDb == null)
            return false;
        _context.Users.Remove(userDb);
        await _context.SaveChangesAsync();
        return true;
    }
}