namespace BL.Models;

public class SyncChatResult
{
    public Guid ChatId { get; set; }

    public List<Message> Messages { get; set; } = new();

    public ulong LastVersion { get; set; }

    public List<User> Participants { get; set; } = new();
    
    public string? ChatName { get; set; }

    public ChatType ChatType { get; set; }

    public ulong LastMessageNum { get; set; }
}