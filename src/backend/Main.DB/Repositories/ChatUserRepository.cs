using Main.BL.Models;
using Main.Application.OutPorts;
using Main.DB.Context;
using Main.DB.Converters;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Repositories;

public class ChatUserRepository : IChatUserRepository
{
    private readonly MainDbContext _context;
    public ChatUserRepository(MainDbContext context)
    {
        _context = context;
    }
    public async Task<ChatUser?> GetAsync(Guid chatId, Guid userId)
    {
        var chatUserDb = await _context.ChatsUsers
            .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        return chatUserDb?.ToDomain();
    }
    public async Task<IEnumerable<ChatUser>> GetChatParticipantsIdsAsync(Guid chatId)
    {
        var participantsDb = await _context.ChatsUsers
            .Where(cu => cu.ChatId == chatId)
            .ToListAsync();
        return participantsDb.Select(cu => cu.ToDomain());
    }
    public async Task<IEnumerable<User>> GetChatParticipantsAsync(Guid chatId)
    {
        var participantsDb = await _context.ChatsUsers
            .Include(cu => cu.User)
            .Where(cu => cu.ChatId == chatId)
            .ToListAsync();
        return participantsDb.Select(cu => cu.User.ToDomain());
    }
    public async Task<IEnumerable<ChatParticipantInfoDto>> GetChatParticipantsInfosAsync(Guid chatId)
    {
        var participantsDb = await _context.ChatsUsers
            .Include(cu => cu.User)
            .Where(cu => cu.ChatId == chatId)
            .ToListAsync();
        return participantsDb.Select(cu => new ChatParticipantInfoDto()
        {
            UserId = cu.UserId,
            UniqueName = cu.User.UniqueName,
            DisplayedName = cu.User.DisplayedName,
            LastMessageRead = cu.LastMessageRead
        });
    }
    public async Task<ulong> GetLastMessageReadAsync(Guid chatId, Guid userId)
    {
        var chatUser = await _context.ChatsUsers
            .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        return chatUser?.LastMessageRead ?? 0;
    }
    public async Task<bool> TryAddAsync(Guid chatId, ChatUser chatUser)
    {
        var exists = await _context.ChatsUsers
            .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == chatUser.UserId);
        if (exists)
            return false;
        var chat = await _context.Chats.FindAsync(chatId);
        if (chat == null)
            return false;
        var chatUserDb = chatUser.ToDb(chatId);
        chatUserDb.LastMessageRead = chat.LastMessageNum;
        await _context.ChatsUsers.AddAsync(chatUserDb);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<int> TryAddManyAsync(Guid chatId, IEnumerable<ChatUser> chatUsers)
    {
        var userIds = chatUsers.Select(u => u.UserId).ToHashSet();
        var existingUserIds = await _context.ChatsUsers
            .Where(cu => cu.ChatId == chatId && userIds.Contains(cu.UserId))
            .Select(cu => cu.UserId)
            .ToHashSetAsync();
        var newUsers = chatUsers.Where(u => !existingUserIds.Contains(u.UserId)).ToList();
        foreach (var user in newUsers)
            _context.ChatsUsers.Add(user.ToDb(chatId));
        await _context.SaveChangesAsync();
        return newUsers.Count;
    }
    public async Task<bool> TryUpdateLastMessageReadAsync(Guid chatId, Guid userId, ulong lastMessageRead)
    {
        var chatUser = await _context.ChatsUsers
            .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        if (chatUser == null)
            return false;
        chatUser.LastMessageRead = lastMessageRead;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> TryRemoveAsync(Guid chatId, Guid userId)
    {
        var chatUser = await _context.ChatsUsers
            .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        if (chatUser == null)
            return false;
        _context.ChatsUsers.Remove(chatUser);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> IsParticipantAsync(Guid chatId, Guid userId)
    {
        return await _context.ChatsUsers
            .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
    }
}