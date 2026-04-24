namespace Http.Dto;

public class ChatSyncDto
{
    public Guid ChatId { get; set; }
    public int Status { get; set; }

    public ChatMetaDto? ChatMeta { get; set; }
    public List<MessageDto>? Messages { get; set; }
    public List<ChatParticipantInfoDto>? Participants { get; set; }
}