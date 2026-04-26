using Realtime.BL.InputPorts;
using Realtime.BL.OutputPorts;

namespace Realtime.BL;

public class ConnectionManager(IConnectionsRepository repository) : IConnectionManager
{
    public Task OnUserConnectedAsync(string userId, string connectionId)
    {
        repository.AddUserConnection(userId, connectionId);
        return Task.CompletedTask;
    }

    public Task OnUserDisconnectedAsync(string userId, string connectionId)
    {
        repository.RemoveUserConnection(userId, connectionId);
        return Task.CompletedTask;
    }
}