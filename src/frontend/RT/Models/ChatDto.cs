using BL.Models;
using Newtonsoft.Json;

namespace RT.Models;

public record ChatDto(
    [JsonProperty] Guid Id,
    [JsonProperty] string? Name,
    [JsonProperty] ChatType Type,
    [JsonProperty] Guid? OwnerId,
    [JsonProperty] DateTimeOffset CreatedAt,
    [JsonProperty] ulong Version,
    [JsonProperty] ulong LastMessageNum);
    
public record FullChatDto(
    [JsonProperty] ChatDto Chat,
    [JsonProperty] List<ChatUserDto> Participants);