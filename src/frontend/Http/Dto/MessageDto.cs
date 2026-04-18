namespace Http.Dto;


public class MessageDto
{
    public long MessageNumber { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool Deleted { get; set; }
    public long Version { get; set; }
    public int Type { get; set; }
    public long? ReplyToMessageNumber { get; set; }
    public Guid? ForwardedFromUserId { get; set; }
}