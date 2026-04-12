using Main.BL.Models;

namespace Main.Application.OutPorts;

public interface IMessageRepository
{
    Task<Message?> GetByNumberAsync(Guid chatId, ulong messageNumber);
    Task<IEnumerable<Message>> GetOlderMessagesAsync(Guid chatId, ulong fromMessageNumber, int limit = 50);
    Task<IEnumerable<Message>> GetNewerMessagesAsync(Guid chatId, ulong fromMessageNumber, int limit = 50);
    Task<IEnumerable<Message>> GetLastMessagesAsync(Guid chatId, int limit = 50);
    Task<IEnumerable<Message>> GetMessagesAfterVersionAsync(Guid chatId, ulong fromVersion);
    Task<ulong> GetLastMessageNumberAsync(Guid chatId);
    Task<int> GetMessagesCountAsync(Guid chatId);
    Task<bool> TryCreateAsync(Message message);
    Task<bool> TryEditTextAsync(Guid chatId, ulong messageNumber, string newText);
    Task<bool> TryDeleteAsync(Guid chatId, ulong messageNumber);
    Task<bool> ExistsAsync(Guid chatId, ulong messageNumber);
}