namespace Http.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    public string DisplayedName { get; set; } = string.Empty;
}