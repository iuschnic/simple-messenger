using System.Text.Json.Serialization;
using Http.Dto;

public class SyncChatResponseDto
{
    [JsonPropertyName("chat")]
    public ChatSyncDto Chat { get; set; }
}