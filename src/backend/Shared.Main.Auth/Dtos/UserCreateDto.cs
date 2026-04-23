using Newtonsoft.Json;

namespace Shared.Main.Auth.Dtos;

public record UserCreateDto(
    [JsonProperty] Guid Id,
    [JsonProperty] string UniqueName,
    [JsonProperty] string DisplayedName);
