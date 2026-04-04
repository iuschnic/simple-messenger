using Main.BL.Models;
using Main.BL.OutPorts;
using Main.DB.Converters;
using Main.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly MainDbContext _context;

    public ChatRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<Chat?> GetByIdAsync(Guid id)
    {
        var chatDb = await _context.Chats
            .Include(c => c.ChatUsers)
            .FirstOrDefaultAsync(c => c.Id == id);
        return chatDb?.ToDomain([.. chatDb.ChatUsers]);
    }
    public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId)
    {
        var chatsDb = await _context.Chats
            .Include(c => c.ChatUsers)
            .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
            .ToListAsync();
        return chatsDb.Select(c => c.ToDomain([.. c.ChatUsers]));
    }
    public async Task CreateAsync(Chat chat)
    {
        var chatDb = chat.ToDb();
        foreach (var p in chat.Participants)
            chatDb.ChatUsers.Add(p.ToDb(chat.Id));
        await _context.Chats.AddAsync(chatDb);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> TryUpdateNameAsync(Guid chatId, string newName)
    {
        var chatDb = await _context.Chats.FindAsync(chatId);
        if (chatDb == null)
            return false;
        chatDb.Name = newName;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> TryDeleteAsync(Guid id)
    {
        var chat = await _context.Chats.FindAsync(id);
        if (chat == null)
            return false;
        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Chats.AnyAsync(c => c.Id == id);
    }
}