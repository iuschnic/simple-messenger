namespace BL.Models;

public class SyncChatResult
{
    public Guid ChatId { get; set; }
    public List<Message> Messages { get; set; }
    public long LastVersion { get; set; }
}