using Main.BL.Models;
namespace Main.BL.InPorts;

public interface IContactService
{
    Task<IEnumerable<Contact>> GetMyContactsAsync(Guid userId);
    Task AddContactAsync(Guid ownerUserId, Guid contactUserId);
    Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId);
}
