using Shared.Main.Realtime;

namespace Realtime.BL.OutputPorts;

public interface IMessageHandler
{
    Task OnMessageReceivedFromBrokerAsync(EventType eventType, string dataJson);
}
