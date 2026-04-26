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

    public Task<Message?> Find(ulong id, Guid chatId)
    {
        MaybeThrow();
        _messages.TryGetValue(id, out var message);
        return Task.FromResult(message);
    }

    public Task<List<Message>> FindChatMessages(Guid chatId)
    {
        MaybeThrow();

        var result = _messages.Values
            .Where(m => m.ChatId == chatId && !m.Deleted)
            .OrderBy(m => m.MessageNumber)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Message> Save(Message message)
    {
        MaybeThrow();

        if (message.MessageNumber == 0)
            message.MessageNumber = _counter++;

        _messages[message.MessageNumber] = message;

        return Task.FromResult(message);
    }

    public Task<Message?> Edit(long id, DateTime editedAt, string newText)
    {
        MaybeThrow();

        var key = (ulong)id;

        if (_messages.TryGetValue(key, out var msg))
        {
            msg.Text = newText;
            msg.EditedAt = editedAt;
            return Task.FromResult<Message?>(msg);
        }

        return Task.FromResult<Message?>(null);
    }

    public Task Delete(long id)
    {
        MaybeThrow();

        var key = (ulong)id;

        if (_messages.TryGetValue(key, out var msg))
        {
            msg.Deleted = true;
        }

        return Task.CompletedTask;
    }

    public Task<long> GetLastMessageNumber(Guid chatId)
    {
        MaybeThrow();

        var last = _messages.Values
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.MessageNumber)
            .FirstOrDefault();

        var result = last != null ? (long)last.MessageNumber : 0;

        return Task.FromResult(result);
    }
}