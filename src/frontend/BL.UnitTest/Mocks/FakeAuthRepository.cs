namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeAuthRepository : IAuthRepository
{
    private readonly Dictionary<Guid, User> _users = new();

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public User Register(string uniqueName, string passwordHash, string email)
    {
        MaybeThrow();

        var user = new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = uniqueName
        };

        _users[user.Id] = user;
        return user;
    }

    public User Authenticate(string uniqueName, string passwordHash)
    {
        MaybeThrow();

        return _users.Values
            .FirstOrDefault(u => u.UniqueName == uniqueName);
    }

    public User Get(Guid id)
    {
        MaybeThrow();

        return _users.TryGetValue(id, out var u) ? u : null;
    }
}