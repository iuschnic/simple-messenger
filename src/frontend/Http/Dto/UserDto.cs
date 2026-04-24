namespace Http.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; }
    public string DisplayName { get; set; }
    public string? AvatarFilePath { get; set; }
}