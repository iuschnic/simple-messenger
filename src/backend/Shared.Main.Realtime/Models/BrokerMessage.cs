using Newtonsoft.Json;

namespace Shared.Main.Realtime.Models;

public record BrokerMessage(
    [JsonProperty] EventType EventType,
    [JsonProperty] string DataJson);
