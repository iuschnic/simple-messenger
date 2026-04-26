using Shared.Main.Auth;

namespace Main.Application.InPorts;

public interface IMessageHandler
{
    Task OnMessageReceivedFromBrokerAsync(EventType eventType, string dataJson);
}
