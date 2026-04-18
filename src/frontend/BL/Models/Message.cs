namespace BL.Models;

public enum MessageType
{
    Regular,
    Reply,
    Forward,
    System
}
public class Message
{
    public long MessageNumber { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool Deleted { get; set; }
    public long Version { get; set; }
    public MessageType Type { get; set; }
}