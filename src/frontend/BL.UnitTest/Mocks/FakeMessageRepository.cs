using BL.Interfaces;
using BL.Models;

public class FakeMessageRepository : IMessageRepository
{
    private readonly Dictionary<ulong, Message> _messages = new();
    private ulong _counter = 1;

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public Message Find(ulong id)
    {
        MaybeThrow();
        return _messages.TryGetValue(id, out var m) ? m : null;
    }

    public List<Message> FindChatMessages(Guid chatId)
    {
        MaybeThrow();

        return _messages.Values
            .Where(m => m.ChatId == chatId && !m.Deleted)
            .OrderBy(m => m.MessageNumber)
            .ToList();
    }

    public Message Save(Message message)
    {
        MaybeThrow();

        if (message.MessageNumber == 0)
            message.MessageNumber = _counter++;

        _messages[message.MessageNumber] = message;
        return message;
    }

    public Message Edit(long id, DateTime editedAt, string newText)
    {
        MaybeThrow();

        var key = (ulong)id;

        if (_messages.TryGetValue(key, out var msg))
        {
            msg.Text = newText;
            msg.EditedAt = editedAt;
            return msg;
        }

        return null;
    }

    public void Delete(long id)
    {
        MaybeThrow();

        var key = (ulong)id;

        if (_messages.TryGetValue(key, out var msg))
        {
            msg.Deleted = true;
        }
    }

    public long GetLastMessageNumber(Guid chatId)
    {
        MaybeThrow();

        var last = _messages.Values
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.MessageNumber)
            .FirstOrDefault();

        return last != null ? (long)last.MessageNumber : 0;
    }
}