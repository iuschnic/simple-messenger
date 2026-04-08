using Shared.Main.Realtime.Models;

namespace Realtime.BL.OutputPorts;

public interface IMessageHandler
{
    Task OnMessageReceivedFromBrokerAsync(BrokerMessage message);
}
