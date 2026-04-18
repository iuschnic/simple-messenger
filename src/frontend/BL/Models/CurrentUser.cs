namespace BL.Models;

public class CurrentUser
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string DisplayedName { get; set; }
}