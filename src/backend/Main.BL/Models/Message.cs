using Main.BL.Enums;
namespace Main.BL.Models;

public class Message(
    ulong messageNumber,
    Guid chatId,
    Guid senderId,
    string text,
    DateTime createdAt,
    DateTime editedAt,
    bool deleted,
    ulong version,
    MessageType type,
    ulong? replyToMessageNumber,
    Guid? forwardedFromUserId
)
{
    public ulong MessageNumber { get; } = messageNumber;
    public Guid ChatId { get; } = chatId;
    public Guid SenderId { get; } = senderId;
    public string Text { get; } = text;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime EditedAt { get; } = editedAt;
    public bool Deleted { get; } = deleted;
    public ulong Version { get; } = version;
    public MessageType Type { get; } = type;
    public ulong? ReplyToMessageNumber { get; } = replyToMessageNumber;
    public Guid? ForwardedFromUserId { get; } = forwardedFromUserId;
}
