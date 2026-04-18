namespace Http.Dto;

public class RegisterRequest
{
    public string UniqueName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string DisplayedName { get; set; }
}