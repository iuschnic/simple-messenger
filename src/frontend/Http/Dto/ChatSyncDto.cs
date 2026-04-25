using System.Text.Json.Serialization;

namespace Http.Dto;

public class ChatSyncDto
{
    [JsonPropertyName("chatId")]
    public Guid ChatId { get; set; }
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("chatMeta")]
    public ChatMetaDto? ChatMeta { get; set; }
    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }
    [JsonPropertyName("participants")]
    public List<ChatParticipantInfoDto>? Participants { get; set; }
}