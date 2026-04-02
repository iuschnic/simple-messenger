using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ContactDb
{
    internal ContactDb() { }
    public ContactDb(Guid ownerUserId,
        Guid contactUserId,
        string contactName)
    {
        OwnerUserId = ownerUserId;
        ContactUserId = contactUserId;
        ContactName = contactName;
    }
    [Required]
    public Guid OwnerUserId { get; set; }  //hard delete при удалении владельца контакта
    [Required]
    public Guid ContactUserId { get; set; }  //hard delete при удалении контакта
    [Required]
    public string ContactName { get; set; }
    public UserDb? OwnerUser {  get; set; }
    public UserDb? ContactUser { get; set; }
}

