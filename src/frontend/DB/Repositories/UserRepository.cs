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

    public User Find(Guid id)
    {
        using var db = _factory.Create();
        return db.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Id = @id",
            new { id });
    }

    public User? FindByUniqueName(string uniqueName)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE UniqueName = @uniqueName",
            new { uniqueName }
        );
    }

    public User? GetByUniqueName(string uniqueName)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE UniqueName = @uniqueName",
            new { uniqueName });
    }

    public List<User> FindContacts(Guid ownerId)
    {
        using var db = _factory.Create();

        return db.Query<User>(@"
            SELECT u.*
            FROM Users u
            JOIN Contacts c ON c.ContactId = u.Id
            WHERE c.OwnerId = @ownerId
        ", new { ownerId }).ToList();
    }

    public User SaveContact(Guid ownerId, string contactUniqueName)
    {
        using var db = _factory.Create();

        var user = FindByUniqueName(contactUniqueName);

        db.Execute(@"
            INSERT OR IGNORE INTO Contacts (OwnerId, ContactId)
            VALUES (@ownerId, @contactId)
        ", new { ownerId, contactId = user.Id });

        return user;
    }

    public User Save(User user)
    {
        using var db = _factory.Create();

        db.Execute(@"
            INSERT OR REPLACE INTO Users (Id, UniqueName, DisplayName, ContactName)
            VALUES (@Id, @UniqueName, @DisplayName, @ContactName)
        ", user);

        return user;
    }

    public User UpdateContactName(Guid userId, string contact)
    {
        using var db = _factory.Create();

        db.Execute(@"
            UPDATE Users SET ContactName = @contact WHERE Id = @userId
        ", new { userId, contact });

        return Find(userId);
    }

    public void Delete(Guid id)
    {
        using var db = _factory.Create();
        db.Execute("DELETE FROM Users WHERE Id = @id", new { id });
    }
    
    public List<User> FindUsersWithContactName()
    {
        using var db = _factory.Create();

        return db.Query<User>(@"
        SELECT *
        FROM Users
        WHERE ContactName IS NOT NULL
          AND ContactName != ''
    ").ToList();
    }
}