namespace Realtime.BL.OutputPorts;

public interface IConnectionManager
{
    Task OnUserConnectedAsync(string userId, string connectionId);
    Task OnUserDisconnectedAsync(string userId, string connectionId);
}
