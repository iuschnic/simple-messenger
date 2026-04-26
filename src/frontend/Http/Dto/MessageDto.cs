using System.Text.Json.Serialization;

namespace Http.Dto;


public class MessageDto
{
    [JsonPropertyName("messageNumber")]
    public long MessageNum { get; set; }
    [JsonPropertyName("chatId")]
    public Guid ChatId { get; set; }
    [JsonPropertyName("senderUserId")]
    public Guid? SenderId { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("editedAt")]
    public DateTime? EditedAt { get; set; }
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
    [JsonPropertyName("version")]
    public long Version { get; set; }
    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    [JsonPropertyName("replyToMessageNumber")]
    public long? ReplyToMessageNum { get; set; }
    [JsonPropertyName("forwardedFromUserId")]
    public Guid? ForwardedFromUserId { get; set; }
}