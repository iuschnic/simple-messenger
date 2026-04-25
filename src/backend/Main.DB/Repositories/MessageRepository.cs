using Main.BL.Models;
using Main.Application.OutPorts;
using Main.DB.Converters;
using Main.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MainDbContext _context;
    public MessageRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<Message?> GetByNumberAsync(Guid chatId, ulong messageNumber)
    {
        var messageDb = await _context.Messages
            .FirstOrDefaultAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
        return messageDb?.ToDomain();
    }
    public async Task<IEnumerable<Message>> GetOlderMessagesAsync(Guid chatId, ulong fromMessageNumber, int limit = 50)
    {
        var messagesDb = await _context.Messages
            .Where(m => m.ChatId == chatId && m.MessageNumber < fromMessageNumber)
            .OrderByDescending(m => m.MessageNumber)
            .Take(limit)
            .OrderBy(m => m.MessageNumber)
            .ToListAsync();

        return messagesDb.Select(m => m.ToDomain());
    }
    public async Task<IEnumerable<Message>> GetNewerMessagesAsync(Guid chatId, ulong fromMessageNumber, int limit = 50)
    {
        var messagesDb = await _context.Messages
            .Where(m => m.ChatId == chatId && m.MessageNumber > fromMessageNumber)
            .OrderBy(m => m.MessageNumber)
            .Take(limit)
            .ToListAsync();

        return messagesDb.Select(m => m.ToDomain());
    }
    public async Task<IEnumerable<Message>> GetLastMessagesAsync(Guid chatId, int limit = 50)
    {
        var messagesDb = await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.MessageNumber)
            .Take(limit)
            .OrderBy(m => m.MessageNumber)
            .ToListAsync();
        return messagesDb.Select(m => m.ToDomain());
    }
    public async Task<IEnumerable<Message>> GetMessagesAfterVersionAsync(Guid chatId, ulong fromVersion)
    {
        var messagesDb = await _context.Messages
            .Where(m => m.ChatId == chatId && m.Version > fromVersion)
            .OrderBy(m => m.Version)
            .ToListAsync();
        return messagesDb.Select(m => m.ToDomain());
    }

    // попытка в optimistic блокировку
    public async Task<ulong?> TryCreateAsync(Message message)
    {
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var chatDb = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Id == message.ChatId);
                if (chatDb == null)
                    return null;
                var newVersion = chatDb.Version + 1;
                var newMessageNumber = chatDb.LastMessageNum + 1;
                chatDb.Version = newVersion;
                chatDb.LastMessageNum = newMessageNumber;
                var messageDb = message.ToDb();
                messageDb.MessageNumber = newMessageNumber;
                messageDb.Version = newVersion;
                await _context.Messages.AddAsync(messageDb);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return newMessageNumber;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                // retry
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }
        return null;
    }
    public async Task<bool> TryEditTextAsync(Guid chatId, ulong messageNumber, string newText)
    {
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var chatDb = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Id == chatId);

                if (chatDb == null)
                    return false;
                var messageDb = await _context.Messages
                    .FirstOrDefaultAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
                if (messageDb == null || messageDb.Deleted)
                    return false;
                if (messageDb.Text == newText)
                    return true;
                var newVersion = chatDb.Version + 1;
                chatDb.Version = newVersion;
                messageDb.Text = newText;
                messageDb.EditedAt = DateTime.UtcNow;
                messageDb.Version = newVersion;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                // retry
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        return false;
    }
    public async Task<bool> TryDeleteAsync(Guid chatId, ulong messageNumber)
    {
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var chatDb = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Id == chatId);
                if (chatDb == null)
                    return false;
                var messageDb = await _context.Messages
                    .FirstOrDefaultAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
                if (messageDb == null || messageDb.Deleted)
                    return false;
                var newVersion = chatDb.Version + 1;
                chatDb.Version = newVersion;
                messageDb.Deleted = true;
                messageDb.Text = string.Empty;
                messageDb.EditedAt = null;
                messageDb.Version = newVersion;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                // retry
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        return false;
    }
    public async Task<bool> ExistsAsync(Guid chatId, ulong messageNumber)
    {
        return await _context.Messages
            .AnyAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
    }
}