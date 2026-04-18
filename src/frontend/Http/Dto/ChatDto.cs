using Http.Dto;

public class ChatDto
{
    public Guid ChatId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Version { get; set; }
    public long LastMessageNum { get; set; }

    public List<UserDto> Members { get; set; } = new();
}