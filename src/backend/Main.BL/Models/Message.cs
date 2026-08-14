using Main.BL.Enums;
using Main.BL.Exceptions;
namespace Main.BL.Models;

public class Message
{
    public ulong MessageNumber { get; private set; }
    public Guid ChatId { get; }
    public Guid? SenderUserId { get; }  //может быть удален или сообщение системное
    public string Text { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? EditedAt { get; private set; }  //может не быть изменено
    public bool Deleted { get; private set; }  //soft-delete в случае "удаления" сообщения самим пользователем
    public ulong Version { get; private set; }
    public MessageType Type { get; }
    public ulong? ReplyToMessageNumber { get; }  //сообщение может быть удалено или сообщение не reply
    public Guid? ForwardedFromUserId { get; }  //user может быть удален или сообщение не forward
    private Message(
        ulong messageNumber,
        Guid chatId,
        Guid? senderUserId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool deleted,
        ulong version,
        MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId)
    {
        CheckText(text);
        EnsureNotEmptyIfPresent(chatId, "chatId");
        EnsureNotEmptyIfPresent(senderUserId, "senderUserId");
        EnsureNotEmptyIfPresent(forwardedFromUserId, "forwardedFromUserId");
        switch (type)
        {
            // Обычное сообщение не должно иметь ссылок на ReplyToMessageNumber или ForwardedFromUserId
            case MessageType.Regular:
                if (replyToMessageNumber != null)
                    throw new DomainValidationException("Regular message cannot be a reply");
                if (forwardedFromUserId != null)
                    throw new DomainValidationException("Regular message cannot be a forwarded");
                break;
            /* Reply-сообщение не должно иметь ссылки на ForwardedFromUserId
             * Может иметь или не иметь ссылку на ReplyToMessageNumber
             * Если ссылка на ReplyToMessageNumber отсутствует, то сообщение-оригинал удалено, это ок
             */
            case MessageType.Reply:
                if (forwardedFromUserId != null)
                    throw new DomainValidationException("Reply message cannot be a forwarded");
                break;
            /* Forwarded-сообщение не должно иметь ссылки на ReplyToMessageNumber 
             * Может иметь или не иметь ссылку на ForwardedFromUserId
             * Если ссылка на ForwardedFromUserId отсутствует, то пользователь-автор сообщения удален, это ок
             */
            case MessageType.Forward:
                if (replyToMessageNumber != null)
                    throw new DomainValidationException("Forwarded message cannot be a reply");
                break;
            /* Системное сообщение не должно иметь SenderId
             * Не должно иметь ссылок на ReplyToMessageNumber или ForwardedFromUserId
             */
            case MessageType.System:
                if (senderUserId != null)
                    throw new DomainValidationException("System message should not have a sender");
                if (replyToMessageNumber != null)
                    throw new DomainValidationException("System message cannot be a reply");
                if (forwardedFromUserId != null)
                    throw new DomainValidationException("System message cannot be a forwarded");
                break;
            default:
                throw new DomainValidationException($"Unknown message type: {type}");
        }
        if (editedAt != null && editedAt < createdAt)
            throw new DomainValidationException("EditedAt cannot be earlier than CreatedAt");
        MessageNumber = messageNumber;
        ChatId = chatId;
        SenderUserId = senderUserId;
        Text = text;
        CreatedAt = createdAt;
        EditedAt = editedAt;
        Deleted = deleted;
        Version = version;
        Type = type;
        ReplyToMessageNumber = replyToMessageNumber;
        ForwardedFromUserId = forwardedFromUserId;
    }
    private static void EnsureNotEmptyIfPresent(Guid? id, string fieldName)
    {
        if (id == Guid.Empty)
            throw new DomainValidationException($"{fieldName} cannot be empty");
    }
    private static void CheckText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainValidationException("Message cannot be empty");
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
            0, // правильная версия будет установлена репозиторием в рамках транзакции
            MessageType.Reply,
            replyToMessageNumber,
            null);
    }
    public static Message CreateForward(
        Guid chatId,
        Guid senderId,
        string text,
        Guid? forwardedFromUserId)  // может быть null, если автор удален
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
            0, // правильная версия будет установлена репозиторием в рамках транзакции
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
    public void EditText(string newText)
    {
        if (Deleted)
            throw new DomainRuleViolationException("Cannot edit a deleted message");
        if (Type == MessageType.Forward)
            throw new DomainRuleViolationException("Cannot edit a forwarded message");

        CheckText(newText);

        Text = newText;
        EditedAt = DateTime.UtcNow;
    }
    public void ApplyMessageNumber(ulong messageNumber)
    {
        // номер должен быть присвоен один раз репозиторием
        if (MessageNumber != 0)
            throw new DomainValidationException("Message number is already assigned");
        MessageNumber = messageNumber;
    }
    public void ApplyNewVersion(ulong newVersion)
    {
        if (newVersion <= Version)
            throw new DomainValidationException("Version can only increase");
        Version = newVersion;
    }
    public void MarkDeleted()
    {
        Deleted = true;
    }
}
