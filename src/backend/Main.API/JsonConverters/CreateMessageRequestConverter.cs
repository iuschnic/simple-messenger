using Main.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Main.API.JsonConverters;

public class CreateMessageRequestConverter : JsonConverter<BaseCreateMessageRequest>
{
    public override BaseCreateMessageRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;
        if (!jsonObject.TryGetProperty("messageType", out var messageTypeProperty))
            throw new JsonException("Missing required property 'messageType'");
        var messageType = messageTypeProperty.GetInt32();
        var jsonText = jsonObject.GetRawText();
        return messageType switch
        {
            0 => JsonSerializer.Deserialize<SendMessageRequest>(jsonText, options),
            1 => JsonSerializer.Deserialize<ReplyMessageRequest>(jsonText, options),
            2 => JsonSerializer.Deserialize<ForwardMessageRequest>(jsonText, options),
            _ => throw new JsonException($"Unknown message type: {messageType}")
        };
    }
    public override void Write(Utf8JsonWriter writer, BaseCreateMessageRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}