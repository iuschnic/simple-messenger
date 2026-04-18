namespace Http.Dto;

public class SyncChatResponseDto
{
    public Guid ChatId { get; set; }
    public List<MessageDto> Messages { get; set; }
    public long LastVersion { get; set; }
}