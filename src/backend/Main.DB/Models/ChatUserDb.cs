using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ChatUserDb
{
    internal ChatUserDb() { }
    public ChatUserDb(Guid chatId,
        Guid userId,
        ulong lastMessageRead)
    {
        ChatId = chatId;
        UserId = userId;
        LastMessageRead = lastMessageRead;
    }
    [Required]
    public Guid ChatId { get; set; }  //hard delete при удалении чата
    [Required]
    public Guid UserId { get; set; }  //hard delete при удалении пользователя
    [Required]
    public ulong LastMessageRead { get; set; }

    public UserDb? User { get; set; }
    public ChatDb? Chat { get; set; }
}
