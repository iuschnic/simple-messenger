namespace Http.Dto;

public class ChatMetaDto
{
    public string? Name { get; set; }
    public int Type { get; set; }
    public Guid? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ulong Version { get; set; }
    public ulong LastMessageNum { get; set; }
}