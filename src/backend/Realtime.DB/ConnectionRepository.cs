using System.Collections.Concurrent;
using Realtime.BL.InputPorts;

namespace Realtime.DB;

public class ConnectionRepository : IConnectionsRepository
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = [];

    public void AddUserConnection(string userId, string connectionId)
    {
        var connections = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);
    }

    public void RemoveUserConnection(string userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var connections))
            return;
        connections.Remove(connectionId, out _);
        if (connections.IsEmpty)
            _connections.TryRemove(userId, out _);
    }

    public IEnumerable<string> GetConnectionIdsByUserId(string userId) => 
        _connections.TryGetValue(userId, out var connections) 
            ? connections.Keys.ToList() 
            : Enumerable.Empty<string>();
}
