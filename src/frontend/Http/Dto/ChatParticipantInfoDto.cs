using System.Text.Json.Serialization;

namespace Http.Dto;

public class ChatParticipantInfoDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; } 
    [JsonPropertyName("displayedName")]
    public string DisplayedName { get; set; } = string.Empty;
    [JsonPropertyName("lastMessageRead")]
    public ulong LastMessageRead { get; set; }
}