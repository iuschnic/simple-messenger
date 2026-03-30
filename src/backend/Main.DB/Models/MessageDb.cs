using Main.DB.Enums;
using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class MessageDb
{
    internal MessageDb() { }
    public MessageDb(
        ulong messageNumber,
        Guid chatId,
        Guid senderId,
        string text,
        MessageTypeDb type,
        ulong? replyToMessageNumber = null,
        Guid? forwardedFromUserId = null)
    {
        MessageNumber = messageNumber;
        ChatId = chatId;
        SenderId = senderId;
        Text = text;
        Type = type;
        ReplyToMessageNumber = replyToMessageNumber;
        ForwardedFromUserId = forwardedFromUserId;
        CreatedAt = DateTime.UtcNow;
        EditedAt = DateTime.UtcNow;
        Deleted = false;
        Version = 1;
    }
    [Required]
    public ulong MessageNumber { get; set; }
    [Required]
    public Guid ChatId { get; set; }
    [Required]
    public Guid SenderId { get; set; }
    [Required]
    public string Text { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime EditedAt { get; set; }
    [Required]
    public bool Deleted { get; set; }
    [Required]
    public ulong Version { get; set; }
    [Required]
    public MessageTypeDb Type { get; set; }

    public ulong? ReplyToMessageNumber { get; set; }
    public Guid? ForwardedFromUserId { get; set; }
    public ChatDb? Chat { get; set; }
    public UserDb? Sender { get; set; }
    public MessageDb? ReplyToMessage { get; set; }
    public UserDb? ForwardedFromUser { get; set; }
}
