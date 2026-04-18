namespace BL.Models;

public class Chat
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long Version { get; set; }
    public string Type { get; set; }
    public long LastMessageNum { get; set; }
    
    public List<User> Members { get; set; } = new();
}