using Newtonsoft.Json;

namespace Shared.Main.Realtime.Models;

public record UserDto(
    [JsonProperty] Guid Id,
    [JsonProperty] string UniqueName,
    [JsonProperty] string DisplayedName);