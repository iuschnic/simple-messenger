namespace Shared.Main.Auth.Dtos;

using Newtonsoft.Json;

public record UserCreateDto(
    [JsonProperty] string UniqueName,
    [JsonProperty] string DisplayedName);
