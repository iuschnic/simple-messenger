using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ContactDb
{
    internal ContactDb() { }
    public ContactDb(Guid ownerIUserId,
        Guid contactUserId,
        string contactName)
    {
        OwnerUserId = ownerIUserId;
        ContactUserId = contactUserId;
        ContactName = contactName;
    }
    [Required]
    public Guid OwnerUserId { get; set; }
    [Required]
    public Guid ContactUserId { get; set; }
    [Required]
    public string ContactName { get; set; }
    public UserDb? UserOwner {  get; set; }
    public UserDb? UserContact { get; set; }
}

