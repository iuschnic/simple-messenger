using Main.BL.Models;
namespace Main.Application.InPorts;
public interface IContactService
{
    Task<IEnumerable<Contact>> GetMyContactsAsync(Guid userId);
    Task AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName);
    Task ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName);
    Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId);
}
