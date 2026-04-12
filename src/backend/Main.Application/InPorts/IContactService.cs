using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IContactService
{
    Task<IEnumerable<ContactWithUser>> GetMyContactsAsync(Guid userId);
    Task AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName);
    Task ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName);
    Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId);
}
