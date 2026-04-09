using Main.BL.Models;

namespace Main.BL.OutPorts;

public interface IMessageRepository
{
    Task<Message?> GetByNumberAsync(Guid chatId, ulong messageNumber);
    Task<IEnumerable<Message>> GetChatMessagesAsync(Guid chatId, ulong fromMessageNumber = 0, int limit = 50);
    Task<IEnumerable<Message>> GetMessagesAfterVersionAsync(Guid chatId, ulong fromVersion);
    Task<ulong> GetLastMessageNumberAsync(Guid chatId);
    Task<int> GetMessagesCountAsync(Guid chatId);
    Task<bool> TryCreateAsync(Message message);
    Task<bool> TryEditTextAsync(Guid chatId, ulong messageNumber, string newText);
    Task<bool> TryDeleteAsync(Guid chatId, ulong messageNumber);
    Task<bool> ExistsAsync(Guid chatId, ulong messageNumber);
}