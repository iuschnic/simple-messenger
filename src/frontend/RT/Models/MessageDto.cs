using BL.Models;
using Newtonsoft.Json;

namespace RT.Models;

public record MessageDto(
    [JsonProperty] ulong MessageNumber,
    [JsonProperty] Guid ChatId,
    [JsonProperty] Guid? SenderId,
    [JsonProperty] string Text,
    [JsonProperty] DateTimeOffset CreatedAt,
    [JsonProperty] DateTimeOffset? EditedAt,
    [JsonProperty] bool IsDeleted,
    [JsonProperty] ulong Version,
    [JsonProperty] MessageType Type,
    [JsonProperty] ulong? ReplyToMessageNumber,
    [JsonProperty] Guid? ForwardedFromUserId);