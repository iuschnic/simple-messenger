using Main.BL.Enums;
namespace Main.BL.Models;

public class Message
{
    public Message(
        ulong messageNumber,
        Guid chatId,
        Guid? senderId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool deleted,
        ulong version,
        MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message cannot be empty");
        switch (type)
        {
            // Обычное сообщение не должно иметь ссылок на ReplyToMessageNumber или ForwardedFromUserId
            case MessageType.Regular:
                if (replyToMessageNumber != null)
                    throw new ArgumentException("Regular message cannot be a reply");
                if (forwardedFromUserId != null)
                    throw new ArgumentException("Regular message cannot be a forwarded");
                break;
            /* Reply-сообщение не должно иметь ссылки на ForwardedFromUserId
             * Может иметь или не иметь ссылку на ReplyToMessageNumber
             * Если ссылка на ReplyToMessageNumber отсутствует, то сообщение-оригинал удалено, это ок
             */
            case MessageType.Reply:
                if (forwardedFromUserId != null)
                    throw new ArgumentException("Reply message cannot be a forwarded");
                break;
            /* Forwarded-сообщение не должно иметь ссылки на ReplyToMessageNumber 
             * Может иметь или не иметь ссылку на ForwardedFromUserId
             * Если ссылка на ForwardedFromUserId отсутствует, то пользователь-автор сообщения удален, это ок
             */
            case MessageType.Forward:
                if (replyToMessageNumber != null)
                    throw new ArgumentException("Forwarded message cannot be a reply");
                break;
            /* Системное сообщение не должно иметь SenderId
             * Не должно иметь ссылок на ReplyToMessageNumber или ForwardedFromUserId
             */
            case MessageType.System:
                if (senderId != null && senderId != Guid.Empty)
                    throw new ArgumentException("System message should not have a sender", nameof(senderId));
                if (replyToMessageNumber != null)
                    throw new ArgumentException("System message cannot be a reply");
                if (forwardedFromUserId != null)
                    throw new ArgumentException("System message cannot be a forwarded");
                break;
            default:
                throw new ArgumentException($"Unknown message type: {type}", nameof(type));
        }
        if (editedAt != null && editedAt < createdAt)
            throw new ArgumentException("EditedAt cannot be earlier than CreatedAt", nameof(editedAt));
        MessageNumber = messageNumber;
        ChatId = chatId;
        SenderId = senderId;
        Text = text;
        CreatedAt = createdAt;
        EditedAt = editedAt;
        Deleted = deleted;
        Version = version;
        Type = type;
        ReplyToMessageNumber = replyToMessageNumber;
        ForwardedFromUserId = forwardedFromUserId;
    }
    public ulong MessageNumber { get; }
    public Guid ChatId { get; }
    public Guid? SenderId { get; }  //может быть удален или сообщение системное
    public string Text { get; }
    public DateTime CreatedAt { get; }
    public DateTime? EditedAt { get; }  //может не быть изменено
    public bool Deleted { get; }  //soft-delete в случае "удаления" сообщения самим пользователем
    public ulong Version { get; }
    public MessageType Type { get; }
    public ulong? ReplyToMessageNumber { get; }  //сообщение может быть удалено или сообщение не reply
    public Guid? ForwardedFromUserId { get; }  //user может быть удален или сообщение не forward
}
