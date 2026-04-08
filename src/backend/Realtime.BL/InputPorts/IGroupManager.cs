namespace Realtime.BL.InputPorts;

public interface IGroupManager
{
    Task SendToGroupAsync(string groupName, string senderId, string eventType, string messageJson);
    Task SendToAllUserGroupsAsync(string userId,  string eventType, string messageJson);
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
}
