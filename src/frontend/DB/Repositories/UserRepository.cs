using Dapper;
using DB.Database;
using BL.Models;
using BL.Interfaces;

namespace DB.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> Find(Guid id)
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @id",
            new { id });
    }

    public async Task<User?> FindByUniqueName(string uniqueName)
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE UniqueName = @uniqueName",
            new { uniqueName }
        );
    }

    public async Task<User?> GetByUniqueName(string uniqueName)
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE UniqueName = @uniqueName",
            new { uniqueName });
    }

    public async Task<List<User>> FindContacts(Guid ownerId)
    {
        using var db = _factory.Create();

        var result = await db.QueryAsync<User>(@"
            SELECT u.*
            FROM Users u
            JOIN Contacts c ON c.ContactId = u.Id
            WHERE c.OwnerId = @ownerId
        ", new { ownerId });

        return result.ToList();
    }

    public async Task<User?> SaveContact(Guid ownerId, string contactUniqueName)
    {
        using var db = _factory.Create();

        var user = await FindByUniqueName(contactUniqueName);

        if (user == null)
            return null;

        await db.ExecuteAsync(@"
            INSERT OR IGNORE INTO Contacts (OwnerId, ContactId)
            VALUES (@ownerId, @contactId)
        ", new { ownerId, contactId = user.Id });

        return user;
    }

    public async Task<User> Save(User user)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            INSERT OR REPLACE INTO Users (Id, UniqueName, DisplayName, ContactName)
            VALUES (@Id, @UniqueName, @DisplayName, @ContactName)
        ", user);

        return user;
    }

    public async Task<User?> UpdateContactName(Guid userId, string contact)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Users SET ContactName = @contact WHERE Id = @userId
        ", new { userId, contact });

        return await Find(userId);
    }

    public async Task Delete(Guid id)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(
            "DELETE FROM Users WHERE Id = @id",
            new { id });
    }

    public async Task<List<User>> FindUsersWithContactName()
    {
        using var db = _factory.Create();

        var result = await db.QueryAsync<User>(@"
            SELECT *
            FROM Users
            WHERE ContactName IS NOT NULL
              AND ContactName != ''
        ");

        return result.ToList();
    }
}