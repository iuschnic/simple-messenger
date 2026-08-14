using Main.Application.Exceptions;
using Main.Application.OutPorts;
using Main.BL.Models;
using Main.DB.Context;
using Main.DB.Converters;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Main.DB.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MainDbContext _context;
    private const int MaxRetries = 3;
    public MessageRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<Message?> GetByNumberAsync(Guid chatId, ulong messageNumber)
    {
        var messageDb = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
        return messageDb?.ToDomain();
    }
    public async Task<IEnumerable<Message>> GetOlderMessagesAsync(Guid chatId, ulong fromMessageNumber, int limit = 50)
    {
        var messagesDb = await _context.Messages
            .AsNoTracking()
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
            .AsNoTracking()
            .Where(m => m.ChatId == chatId && m.MessageNumber > fromMessageNumber)
            .OrderBy(m => m.MessageNumber)
            .Take(limit)
            .ToListAsync();

        return messagesDb.Select(m => m.ToDomain());
    }
    public async Task<IEnumerable<Message>> GetLastMessagesAsync(Guid chatId, int limit = 50)
    {
        var messagesDb = await _context.Messages
            .AsNoTracking()
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
            .AsNoTracking()
            .Where(m => m.ChatId == chatId && m.Version > fromVersion)
            .OrderBy(m => m.Version)
            .ToListAsync();
        return messagesDb.Select(m => m.ToDomain());
    }

    public async Task CreateAsync(Message message)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var chatDb = await _context.Chats.FirstOrDefaultAsync(c => c.Id == message.ChatId)
                    ?? throw new NotFoundException($"Chat {message.ChatId} not found");

                var newVersion = chatDb.Version + 1;
                var newMessageNumber = chatDb.LastMessageNum + 1;

                chatDb.Version = newVersion;
                chatDb.LastMessageNum = newMessageNumber;

                var messageDb = message.ToDb();
                messageDb.MessageNumber = newMessageNumber;
                messageDb.Version = newVersion;

                _context.Messages.Add(messageDb);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                message.ApplyMessageNumber(newMessageNumber);
                message.ApplyNewVersion(newVersion);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                await Task.Delay(50 * (attempt + 1));
            }
        }
        throw new ConcurrencyException($"Failed to create message in chat {message.ChatId} after {MaxRetries} attempts");
    }

    public async Task UpdateAsync(Message message)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var chatDb = await _context.Chats.FirstOrDefaultAsync(c => c.Id == message.ChatId)
                    ?? throw new NotFoundException($"Chat {message.ChatId} not found");

                var messageDb = await _context.Messages
                    .FirstOrDefaultAsync(m => m.ChatId == message.ChatId
                                           && m.MessageNumber == message.MessageNumber)
                    ?? throw new NotFoundException($"Message {message.MessageNumber} not found");

                var newVersion = chatDb.Version + 1;
                chatDb.Version = newVersion;

                messageDb.Text = message.Text;
                messageDb.EditedAt = message.EditedAt;
                messageDb.Deleted = message.Deleted;
                messageDb.Version = newVersion;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                message.ApplyNewVersion(newVersion);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                await Task.Delay(50 * (attempt + 1));
            }
        }
        throw new ConcurrencyException($"Failed to update message {message.MessageNumber} after {MaxRetries} attempts");
    }
    public async Task<bool> ExistsAsync(Guid chatId, ulong messageNumber)
    {
        return await _context.Messages
            .AnyAsync(m => m.ChatId == chatId && m.MessageNumber == messageNumber);
    }
}