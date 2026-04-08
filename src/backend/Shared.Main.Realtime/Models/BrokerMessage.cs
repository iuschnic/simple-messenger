namespace Shared.Main.Realtime.Models;

public record BrokerMessage(string EventType, object Data);
