using BL.Interfaces;
using Dapper;
using DB.Database;
using BL.Models;

namespace DB.Repositories;

public class CurrentUserRepository : ICurrentUserRepository
{
    private readonly DbConnectionFactory _factory;

    public CurrentUserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<CurrentUser?> Save(CurrentUser user)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            DELETE FROM CurrentUser;

            INSERT INTO CurrentUser (Id, UniqueName, Email, PasswordHash, DisplayedName)
            VALUES (@Id, @UniqueName, @Email, @PasswordHash, @DisplayedName);
        ", new
        {
            user.Id,
            user.UniqueName,
            user.Email,
            user.PasswordHash,
            user.DisplayedName
        });

        return await db.QueryFirstOrDefaultAsync<CurrentUser>(
            "SELECT * FROM CurrentUser LIMIT 1");
    }

    public async Task<CurrentUser?> Get()
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<CurrentUser>(
            "SELECT * FROM CurrentUser LIMIT 1");
    }

    public async Task Clear()
    {
        using var db = _factory.Create();

        await db.ExecuteAsync("DELETE FROM CurrentUser");
    }
}