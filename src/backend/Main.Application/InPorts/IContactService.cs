using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IContactService
{
    Task<IEnumerable<ContactWithUser>> GetMyContactsAsync(Guid userId);
    Task<ContactWithUser> AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName);
    Task<ContactWithUser> ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName);
    Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId);
}
