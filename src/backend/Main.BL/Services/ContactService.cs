using Main.BL.Exceptions;
using Main.BL.Models;
using Main.BL.OutPorts;
using Main.BL.InPorts;

namespace Main.BL.Services;

public class ContactService: IContactService
{
    private readonly IUserRepository _userRepo;
    private readonly IContactRepository _contactRepo;

    public ContactService(
        IUserRepository userRepo,
        IContactRepository contactRepo)
    {
        _userRepo = userRepo;
        _contactRepo = contactRepo;
    }
    public async Task<IEnumerable<Contact>> GetMyContactsAsync(Guid userId)
    {
        await EnsureUserExists(userId);
        return await _contactRepo.GetUserContactsAsync(userId);
    }
    public async Task AddContactAsync(Guid ownerUserId, Guid contactUserId, string contactName)
    {
        await EnsureUserExists(ownerUserId);
        await EnsureUserExists(contactUserId);
        if (await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact already exists");
        if (!await _contactRepo.TryAddAsync(ownerUserId, new Contact(contactUserId, contactName)))
            throw new TechnicalException("Failed to add contact");
    }
    public async Task ChangeContactNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName)
    {
        await EnsureUserExists(ownerUserId);
        await EnsureUserExists(contactUserId);
        if (!await _contactRepo.ExistsAsync(ownerUserId, contactUserId))
            throw new ConflictException("Contact doesnt exist");
        if (!await _contactRepo.TryUpdateNameAsync(ownerUserId, contactUserId, newContactName))
            throw new TechnicalException("Failed to update contact");
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
    private async Task EnsureUserExists(Guid userId)
    {
        if (!await _userRepo.ExistsAsync(userId))
            throw new NotFoundException(nameof(User), userId);
    }
}

