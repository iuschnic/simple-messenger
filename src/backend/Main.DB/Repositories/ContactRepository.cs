using Main.Application.Converters;
using Main.Application.Dtos;
using Main.Application.OutPorts;
using Main.BL.Models;
using Main.DB.Context;
using Main.DB.Converters;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly MainDbContext _context;

    public ContactRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<Contact?> GetAsync(Guid ownerUserId, Guid contactUserId)
    {
        var contactDb = await _context.Contacts
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId && c.ContactUserId == contactUserId);
        return contactDb?.ToDomain();
    }
    public async Task<IEnumerable<Contact>> GetContactsAsync(Guid ownerUserId)
    {
        var contactsDb = await _context.Contacts
            .Where(c => c.OwnerUserId == ownerUserId)
            .OrderBy(c => c.ContactName)
            .ToListAsync();
        return contactsDb.Select(c => c.ToDomain());
    }
    public async Task<IEnumerable<ContactWithUserDto>> GetContactsWithUserAsync(Guid ownerUserId)
    {
        var contactsDb = await _context.Contacts
            .Include(c => c.ContactUser)
            .Where(c => c.OwnerUserId == ownerUserId)
            .OrderBy(c => c.ContactName)
            .ToListAsync();
        return contactsDb.Select(c => new ContactWithUserDto
        {
            ContactUser = c.ContactUser.ToDomain(),
            ContactName = c.ContactName
        });
    }
    public async Task<bool> ExistsAsync(Guid ownerUserId, Guid contactUserId)
    {
        return await _context.Contacts
            .AnyAsync(c => c.OwnerUserId == ownerUserId && c.ContactUserId == contactUserId);
    }
    public async Task<bool> TryAddAsync(Guid ownerUserId, Contact contact)
    {
        var exists = await _context.Contacts
            .AnyAsync(c => c.OwnerUserId == ownerUserId && c.ContactUserId == contact.ContactUserId);
        if (exists)
            return false;
        var contactDb = contact.ToDb(ownerUserId);
        await _context.Contacts.AddAsync(contactDb);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> TryUpdateNameAsync(Guid ownerUserId, Guid contactUserId, string newContactName)
    {
        var contactDb = await _context.Contacts
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId && c.ContactUserId == contactUserId);
        if (contactDb == null)
            return false;
        if (string.IsNullOrWhiteSpace(newContactName))
            return false;
        contactDb.ContactName = newContactName;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> TryRemoveAsync(Guid ownerUserId, Guid contactUserId)
    {
        var contactDb = await _context.Contacts
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId && c.ContactUserId == contactUserId);
        if (contactDb == null)
            return false;
        _context.Contacts.Remove(contactDb);
        await _context.SaveChangesAsync();
        return true;
    }
}