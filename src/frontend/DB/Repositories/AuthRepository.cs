using BL.Interfaces;
using BL.Models;
using Dapper;
using DB.Database;


namespace DB.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly DbConnectionFactory _factory;

    public AuthRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public User Register(string uniqueName, string passwordHash, string email)
    {
        using var db = _factory.Create();

        var user = new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = uniqueName
        };

        db.Execute(@"
            INSERT INTO Users (Id, UniqueName, DisplayName)
            VALUES (@Id, @UniqueName, @DisplayName)
        ", user);

        return user;
    }

    public User Authenticate(string uniqueName, string passwordHash)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE UniqueName = @uniqueName",
            new { uniqueName });
    }

    public User Get(Guid id)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Id = @id",
            new { id });
    }
}