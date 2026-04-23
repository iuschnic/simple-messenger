using Newtonsoft.Json;

namespace Shared.Main.Auth.Dtos;

public record UserRemoveDto(
    [JsonProperty] Guid Id);
