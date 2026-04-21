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

    public User Find(Guid id)
    {
        MaybeThrow();
        return _users.TryGetValue(id, out var u) ? u : null;
    }

    public User? FindByUniqueName(string uniqueName)
    {
        MaybeThrow();
        return _users.Values.FirstOrDefault(u => u.UniqueName == uniqueName);
    }

    public User? GetByUniqueName(string uniqueName)
    {
        MaybeThrow();
        return _users.Values.FirstOrDefault(u => u.UniqueName == uniqueName);
    }

    public User Save(User user)
    {
        MaybeThrow();
        _users[user.Id] = user;
        return user;
    }

    public User UpdateContactName(Guid userId, string contact)
    {
        MaybeThrow();

        if (_users.TryGetValue(userId, out var user))
        {
            user.ContactName = contact;
            return user;
        }

        return null;
    }

    public void Delete(Guid id)
    {
        MaybeThrow();

        _users.Remove(id);

        foreach (var list in _contacts.Values)
        {
            list.Remove(id);
        }
    }

    // ================= CONTACTS =================

    public List<User> FindContacts(Guid ownerId)
    {
        MaybeThrow();

        if (!_contacts.ContainsKey(ownerId))
            return new List<User>();

        return _contacts[ownerId]
            .Where(id => _users.ContainsKey(id))
            .Select(id => _users[id])
            .ToList();
    }

    public User SaveContact(Guid ownerId, string contactUniqueName)
    {
        MaybeThrow();

        var user = FindByUniqueName(contactUniqueName);

        if (user == null)
            return null;

        if (!_contacts.ContainsKey(ownerId))
            _contacts[ownerId] = new List<Guid>();

        if (!_contacts[ownerId].Contains(user.Id))
            _contacts[ownerId].Add(user.Id);

        return user;
    }

    public List<User> FindUsersWithContactName()
    {
        MaybeThrow();

        return _users.Values
            .Where(u => !string.IsNullOrEmpty(u.ContactName))
            .ToList();
    }
}