using Main.BL.Enums;
namespace Main.BL.Models;

public class Message
{
    private Message(
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
    public static Message CreateRegular(
        Guid chatId,
        Guid senderId,
        string text)
    {
        return new Message(
            0, // правильный номер будет установлен репозиторием в рамках транзакции
            chatId,
            senderId,
            text,
            DateTime.UtcNow,
            null,
            false,
            0, // правильная версия будет установлена репозиторием в рамках транзакции
            MessageType.Regular,
            null,
            null);
    }
    public static Message CreateReply(
        Guid chatId,
        Guid senderId,
        string text,
        ulong replyToMessageNumber)
    {
        return new Message(
            0, // правильный номер будет установлен репозиторием в рамках транзакции
            chatId,
            senderId,
            text,
            DateTime.UtcNow,
            null,
            false,
            0,
            MessageType.Reply,
            replyToMessageNumber,
            null);
    }
    public static Message CreateForward(
        Guid chatId,
        Guid senderId,
        string text,
        Guid? forwardedFromUserId = null)  // может быть null, если автор удален
    {
        return new Message(
            0, // правильный номер будет установлен репозиторием в рамках транзакции
            chatId,
            senderId,
            text,
            DateTime.UtcNow,
            null,
            false,
            1,
            MessageType.Forward,
            null,
            forwardedFromUserId);
    }
    public static Message CreateSystem(
        Guid chatId,
        string text)
    {
        return new Message(
            0, // правильный номер будет установлен репозиторием в рамках транзакции
            chatId,
            null,
            text,
            DateTime.UtcNow,
            null,
            false,
            0,
            MessageType.System,
            null,
            null);
    }
    public static Message Create(
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
        return new Message(
            messageNumber,
            chatId,
            senderId,
            text,
            createdAt,
            editedAt,
            deleted,
            version,
            type,
            replyToMessageNumber,
            forwardedFromUserId);
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
