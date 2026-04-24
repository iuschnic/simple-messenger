namespace Realtime.BL.InputPorts;

public interface IHttpChatReceiver
{
    public Task<IEnumerable<Guid>> GetAllChatsByUserIdAsync(string userId);
}