using Newtonsoft.Json;

namespace Shared.Main.Realtime.Models;

public record ChatUserDto(
    [JsonProperty] Guid ChatId,
    [JsonProperty] UserDto User, 
    [JsonProperty] ulong LastMessageRead);