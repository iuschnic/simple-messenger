using System.Text.Json.Serialization;

namespace Http.Dto;

public class ChatMetaDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("type")]
    public int Type { get; set; }
    [JsonPropertyName("ownerUserId")]
    public Guid? OwnerUserId { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("version")]
    public ulong Version { get; set; }
    [JsonPropertyName("lastMessageNum")]
    public ulong LastMessageNum { get; set; }
}