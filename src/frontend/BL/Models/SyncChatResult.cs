namespace BL.Models;

public class SyncChatResult
{
    public Guid ChatId { get; set; }

    public List<Message> Messages { get; set; } = new();

    public ulong LastVersion { get; set; }

    // ✅ добавляем пользователей
    public List<User> Participants { get; set; } = new();
}