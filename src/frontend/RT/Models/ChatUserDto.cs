using Newtonsoft.Json;

namespace RT.Models;

public record ChatUserDto(
    [JsonProperty] Guid ChatId,
    [JsonProperty] UserDto User, 
    [JsonProperty] ulong LastMessageRead);