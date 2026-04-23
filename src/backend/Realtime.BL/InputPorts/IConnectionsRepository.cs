namespace Realtime.BL.InputPorts;

public interface IConnectionsRepository
{
    void AddUserConnection(string userId, string connectionId);
    void RemoveUserConnection(string userId, string connectionId);
    IEnumerable<string> GetConnectionIdsByUserId(string userId);
}
