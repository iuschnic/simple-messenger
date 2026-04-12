using Main.BL.Models;

namespace Main.Application.OutPorts;

public interface IContactRepository
{
    Task<Contact?> GetAsync(Guid ownerUserId, Guid contactUserId);
    Task<IEnumerable<Contact>> GetUserContactsAsync(Guid ownerUserId);
    Task<bool> ExistsAsync(Guid ownerUserId, Guid contactUserId);

    Task<bool> TryAddAsync(Guid ownerUserId, Contact contact);

    Task<bool> TryUpdateNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName);

    Task<bool> TryRemoveAsync(Guid ownerUserId, Guid contactUserId);
}