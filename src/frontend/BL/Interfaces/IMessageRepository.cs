using BL.Models;

namespace BL.Interfaces;

public interface IMessageRepository
{
    Task<Message?> Find(ulong id, Guid chatId);
    Task<List<Message>> FindChatMessages(Guid chatId);

    Task<Message> Save(Message message);
    Task<Message?> Edit(long id, DateTime editedAt, string newText);

    Task Delete(long id);
    Task<long> GetLastMessageNumber(Guid chatId);
}