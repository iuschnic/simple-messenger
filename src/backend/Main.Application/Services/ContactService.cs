using Main.BL.Exceptions;
using Main.BL.Models;
using Main.Application.Dtos;
using Main.Application.OutPorts;
using Main.Application.InPorts;

namespace Main.Application.Services;

public class ContactService: BaseService, IContactService
{
    private readonly IContactRepository _contactRepo;

    public ContactService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo,
        IContactRepository contactRepo) : base(userRepo, chatRepo, chatUserRepo, messageRepo)
    {
        _contactRepo = contactRepo;
    }
    public async Task<IEnumerable<ContactWithUser>> GetMyContactsAsync(Guid userId)
    {
        await EnsureUserExists(userId);
        return await _contactRepo.GetContactsWithUserAsync(userId);
    }
    public async Task<ContactWithUser> AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName)
    {
        await EnsureUserExists(ownerUserId);
        var contactUser = await GetUserOrThrow(contactUserId);
        if (await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact already exists");
        if (!await _contactRepo.TryAddAsync(ownerUserId, new Contact(contactUserId, contactName)))
            throw new TechnicalException("Failed to add contact");
        return new ContactWithUser
        {
            ContactUser = contactUser,
            ContactName = contactName
        };
    }
    public async Task<ContactWithUser> ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName)
    {
        await EnsureUserExists(ownerUserId);
        var contactUser = await GetUserOrThrow(contactUserId);
        if (!await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact doesnt exist");
        if (!await _contactRepo.TryUpdateNameAsync(ownerUserId, contactUserId, newContactName))
            throw new TechnicalException("Failed to update contact");
        return new ContactWithUser
        {
            ContactUser = contactUser,
            ContactName = newContactName
        };
    }
    public async Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId)
    {
        await EnsureUserExists(ownerUserId);
        await EnsureUserExists(contactUserId);
        if (!await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact doesnt exist");
        if (!await _contactRepo.TryRemoveAsync(ownerUserId, contactUserId))
            throw new TechnicalException("Failed to remove contact");
    }
}

