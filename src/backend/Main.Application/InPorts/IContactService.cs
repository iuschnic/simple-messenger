using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IContactService
{
    Task<IEnumerable<ContactWithUserDto>> GetMyContactsAsync(Guid userId);
    Task<ContactWithUserDto> AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName);
    Task<ContactWithUserDto> ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName);
    Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId);
}
