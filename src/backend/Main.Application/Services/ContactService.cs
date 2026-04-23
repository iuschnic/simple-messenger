using Main.Application.Dtos;
using Main.Application.Exceptions;
using Main.Application.InPorts;
using Main.Application.Mappers;
using Main.Application.OutPorts;
using Main.BL.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Main.Application.Services;

public class ContactService: BaseService, IContactService
{
    private readonly IContactRepository _contactRepo;

    public ContactService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo,
        IContactRepository contactRepo,
        IMessageProducer messageProducer) : base(userRepo, chatRepo, chatUserRepo, messageRepo, messageProducer)
    {
        _contactRepo = contactRepo;
    }
    public async Task<IEnumerable<ContactWithUserDto>> GetMyContactsAsync(Guid userId)
    {
        await EnsureUserExists(userId);
        return await _contactRepo.GetContactsWithUserAsync(userId);
    }
    public async Task<ContactWithUserDto> AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName)
    {
        if (string.IsNullOrEmpty(contactName))
            throw new ArgumentException("contactName should not be null/whitespace");
        await EnsureCurrentUserAuthorized(ownerUserId);
        var contactUser = await GetUserOrNotFound(contactUserId);
        if (await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact already exists");
        if (!await _contactRepo.TryAddAsync(ownerUserId, new Contact(contactUserId, contactName)))
            throw new TechnicalException("Failed to add contact");

        return new ContactWithUserDto
        {
            ContactUser = contactUser.ToDto(),
            ContactName = contactName
        };
    }
    public async Task<ContactWithUserDto> ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName)
    {
        if (string.IsNullOrEmpty(newContactName))
            throw new ArgumentException("newContactName should not be null/whitespace");
        await EnsureCurrentUserAuthorized(ownerUserId);
        var contactUser = await GetUserOrNotFound(contactUserId);
        var contact = await GetContactOrThrow(ownerUserId, contactUserId);

        if (!await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact doesnt exist");
        if (!await _contactRepo.TryUpdateNameAsync(ownerUserId, contactUserId, newContactName))
            throw new TechnicalException("Failed to update contact");
        return contact.ToDto(contactUser);
    }
    public async Task RemoveContactAsync(Guid ownerUserId, Guid contactUserId)
    {
        await EnsureCurrentUserAuthorized(ownerUserId);
        await EnsureUserExists(contactUserId);
        if (!await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact doesnt exist");
        if (!await _contactRepo.TryRemoveAsync(ownerUserId, contactUserId))
            throw new TechnicalException("Failed to remove contact");
    }
    private async Task<Contact> GetContactOrThrow(Guid ownerUserId, Guid contactUserId)
    {
        return await _contactRepo.GetAsync(ownerUserId, contactUserId)
            ?? throw new NotFoundException(nameof(Contact), ownerUserId.ToString() + " " + contactUserId.ToString());
    }
}

