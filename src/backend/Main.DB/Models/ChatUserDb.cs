using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ChatUserDb
{
    internal ChatUserDb() { }
    public ChatUserDb(Guid chatId,
        Guid userId)
    {
        ChatId = chatId;
        UserId = userId;
    }
    [Required]
    public Guid ChatId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public ulong LastMessageRead { get; set; }

    public UserDb? User { get; set; }
    public ChatDb? Chat { get; set; }
}
