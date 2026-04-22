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

    public async Task<User> Register(string uniqueName, string passwordHash, string email)
    {
        MaybeThrow();

        var user = new User
        {
            Id = Guid.NewGuid(),
            UniqueName = uniqueName,
            DisplayName = uniqueName
        };

        _users[user.Id] = user;

        return await Task.FromResult(user);
    }

    public async Task<User?> Authenticate(string uniqueName, string passwordHash)
    {
        MaybeThrow();

        var user = _users.Values
            .FirstOrDefault(u => u.UniqueName == uniqueName);

        return await Task.FromResult(user);
    }

    public async Task<User?> Get(Guid id)
    {
        MaybeThrow();

        _users.TryGetValue(id, out var user);

        return await Task.FromResult(user);
    }
}