namespace Http.Dto;


public class MessageDto
{
    public ulong MessageNum { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool Deleted { get; set; }
    public ulong Version { get; set; }
    public int Type { get; set; }

    public ulong? ReplyToMessageNum { get; set; }
    public Guid? ForwardedFromUserId { get; set; }
}