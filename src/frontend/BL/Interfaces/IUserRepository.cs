using BL.Models;

namespace BL.Interfaces;

public interface IUserRepository
{
    User Find(Guid id);
    User FindByUniqueName(string uniqueName);
    public User? GetByUniqueName(string uniqueName);

    List<User> FindContacts(Guid ownerId);

    User Save(User user);
    User SaveContact(Guid ownerId, string contactUniqueName);

    public User UpdateContactName(Guid userId, string contact);

    void Delete(Guid id);
    public List<User> FindUsersWithContactName();
}