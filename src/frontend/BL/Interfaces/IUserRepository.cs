using BL.Models;

namespace BL.Interfaces;

public interface IUserRepository
{
    Task<User?> Find(Guid id);
    Task<User?> FindByUniqueName(string uniqueName);
    Task<User?> GetByUniqueName(string uniqueName);

    Task<List<User>> FindContacts(Guid ownerId);

    Task<User> Save(User user);
    Task<User?> SaveContact(Guid ownerId, string contactUniqueName);

    Task<User?> UpdateContactName(Guid userId, string contact);

    Task Delete(Guid id);
    Task<List<User>> FindUsersWithContactName();
}