using Main.DB.Enums;
using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class MessageDb
{
    internal MessageDb() { }
    public MessageDb(
        ulong messageNumber,
        Guid chatId,
        Guid? senderUserId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool deleted,
        ulong version,
        MessageTypeDb type,
        ulong? replyToMessageNumber = null,
        Guid? forwardedFromUserId = null)
    {
        MessageNumber = messageNumber;
        ChatId = chatId;
        SenderUserId = senderUserId;
        Text = text;
        Type = type;
        CreatedAt = createdAt;
        EditedAt = editedAt;
        Deleted = deleted;
        Version = version;
        ReplyToMessageNumber = replyToMessageNumber;
        ForwardedFromUserId = forwardedFromUserId;
    }
    [Required]
    public ulong MessageNumber { get; set; }
    [Required]
    public Guid ChatId { get; set; }  //hard delete при удалении чата
    public Guid? SenderUserId { get; set; }  //пользователь может быть удален при не удаленном чате
    [Required]
    public string Text { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }  //может не быть изменено
    [Required]
    public bool Deleted { get; set; }  //soft-delete для сообщений при существующем чате
    [Required]
    public ulong Version { get; set; }
    [Required]
    public MessageTypeDb Type { get; set; }
    public ulong? ReplyToMessageNumber { get; set; }
    public Guid? ForwardedFromUserId { get; set; }

    public ChatDb? Chat { get; set; }
    public UserDb? SenderUser { get; set; }
    public MessageDb? ReplyToMessage { get; set; }
    public UserDb? ForwardedFromUser { get; set; }
}
