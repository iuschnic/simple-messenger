using Main.Application.Enums;
namespace Main.Application.Dtos;

public class MessageDto
{
    public ulong MessageNumber { get; init; }
    public Guid ChatId { get; init; }
    public Guid? SenderUserId { get; init; }
    public string Text { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? EditedAt { get; init; }
    public bool Deleted { get; init; }
    public ulong Version { get; init; }
    public MessageTypeApp Type { get; init; }
    public ulong? ReplyToMessageNumber { get; init; }
    public Guid? ForwardedFromUserId { get; init; }
}

