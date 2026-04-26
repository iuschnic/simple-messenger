using Main.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Main.API.JsonConverters;

public class CreateChatRequestConverter : JsonConverter<BaseCreateChatRequest>
{
    public override BaseCreateChatRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;
        if (!jsonObject.TryGetProperty("chatType", out var chatTypeProperty))
            throw new JsonException("Missing required property 'chatType'");
        var chatType = chatTypeProperty.GetInt32();
        var jsonText = jsonObject.GetRawText();
        return chatType switch
        {
            0 => JsonSerializer.Deserialize<CreateGroupChatRequest>(jsonText, options),
            1 => JsonSerializer.Deserialize<CreatePrivateChatRequest>(jsonText, options),
            _ => throw new JsonException($"Unknown chat type: {chatType}")
        };
    }
    public override void Write(Utf8JsonWriter writer, BaseCreateChatRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
