namespace Http.Dto;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; }
    public string PasswordHash { get; set; }
    public string DisplayedName { get; set; }
    public string Email { get; set; }
}