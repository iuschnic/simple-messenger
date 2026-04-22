using BL.Interfaces;
using BL.Models;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<Guid, List<Guid>> _contacts = new();

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public Task<User?> Find(Guid id)
    {
        MaybeThrow();
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> FindByUniqueName(string uniqueName)
    {
        MaybeThrow();
        var user = _users.Values.FirstOrDefault(u => u.UniqueName == uniqueName);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUniqueName(string uniqueName)
    {
        MaybeThrow();
        var user = _users.Values.FirstOrDefault(u => u.UniqueName == uniqueName);
        return Task.FromResult(user);
    }

    public Task<User> Save(User user)
    {
        MaybeThrow();
        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task<User?> UpdateContactName(Guid userId, string contact)
    {
        MaybeThrow();

        if (_users.TryGetValue(userId, out var user))
        {
            user.ContactName = contact;
            return Task.FromResult<User?>(user);
        }

        return Task.FromResult<User?>(null);
    }

    public Task Delete(Guid id)
    {
        MaybeThrow();

        _users.Remove(id);

        foreach (var list in _contacts.Values)
        {
            list.Remove(id);
        }

        return Task.CompletedTask;
    }

    // ================= CONTACTS =================

    public Task<List<User>> FindContacts(Guid ownerId)
    {
        MaybeThrow();

        if (!_contacts.ContainsKey(ownerId))
            return Task.FromResult(new List<User>());

        var result = _contacts[ownerId]
            .Where(id => _users.ContainsKey(id))
            .Select(id => _users[id])
            .ToList();

        return Task.FromResult(result);
    }

    public Task<User?> SaveContact(Guid ownerId, string contactUniqueName)
    {
        MaybeThrow();

        var user = _users.Values.FirstOrDefault(u => u.UniqueName == contactUniqueName);

        if (user == null)
            return Task.FromResult<User?>(null);

        if (!_contacts.ContainsKey(ownerId))
            _contacts[ownerId] = new List<Guid>();

        if (!_contacts[ownerId].Contains(user.Id))
            _contacts[ownerId].Add(user.Id);

        return Task.FromResult<User?>(user);
    }

    public Task<List<User>> FindUsersWithContactName()
    {
        MaybeThrow();

        var result = _users.Values
            .Where(u => !string.IsNullOrEmpty(u.ContactName))
            .ToList();

        return Task.FromResult(result);
    }
}