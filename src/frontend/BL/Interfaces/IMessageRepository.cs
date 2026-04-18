using BL.Models;

namespace BL.Interfaces;

public interface IMessageRepository
{
    Message Find(long id);
    List<Message> FindChatMessages(Guid chatId);

    Message Save(Message message);
    Message Edit(long id, DateTime editedAt, string newText);

    void Delete(long id);
    public long GetLastMessageNumber(Guid chatId);
}