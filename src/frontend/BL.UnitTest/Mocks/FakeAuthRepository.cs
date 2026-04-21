namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeAuthRepository : IAuthRepository
{
    private readonly Dictionary<Guid, User> _users = new();

    public User Register(string uniqueName, string passwordHash, string email)
    {
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
        => _users.Values.FirstOrDefault(u => u.UniqueName == uniqueName);

    public User Get(Guid id)
        => _users.TryGetValue(id, out var u) ? u : null;
}