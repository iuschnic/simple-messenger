using Newtonsoft.Json;

namespace RT.Models;

public record UserDto(
    [JsonProperty] Guid Id,
    [JsonProperty] string UniqueName,
    [JsonProperty] string DisplayedName);