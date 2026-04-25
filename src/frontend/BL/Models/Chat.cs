namespace BL.Models;

public enum ChatType
{
    Group,
    Private
}
public class Chat
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public ulong Version { get; set; }
    public ChatType Type { get; set; }
    public ulong LastMessageNum { get; set; }
    
    public List<User> Members { get; set; } = new();
}