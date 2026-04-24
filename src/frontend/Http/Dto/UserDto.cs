namespace Http.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; } = null!;
    public string DisplayedName { get; set; } = null!;
}