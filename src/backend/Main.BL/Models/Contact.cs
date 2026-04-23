namespace Main.BL.Models;

public class Contact(Guid contactUserId, 
    string contactName)
{
    public Guid ContactUserId { get; } = contactUserId;
    public string ContactName { get; } = contactName;
}
