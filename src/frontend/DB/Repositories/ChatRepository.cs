using BL.Interfaces;
using Dapper;
using DB.Database;
using BL.Models;

namespace DB.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly DbConnectionFactory _factory;

    public ChatRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Chat?> Find(Guid id)
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<Chat>(
            "SELECT * FROM Chats WHERE Id = @id",
            new { id });
    }

    public async Task<List<Chat>> GetAllChats()
    {
        using var db = _factory.Create();

        var result = await db.QueryAsync<Chat>(@"
            SELECT *
            FROM Chats
        ");

        return result.ToList();
    }

    public async Task<List<User>> FindChatUsers(Guid chatId)
    {
        using var db = _factory.Create();

        var result = await db.QueryAsync<User>(@"
            SELECT u.*
            FROM Users u
            JOIN ChatsUsers cu ON cu.UserId = u.Id
            WHERE cu.ChatId = @chatId
        ", new { chatId });

        return result.ToList();
    }

    public async Task AddUserToChat(Guid chatId, Guid userId)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            INSERT OR IGNORE INTO ChatsUsers (ChatId, UserId, LastReadMessageNum)
            VALUES (@chatId, @userId, 0)
        ", new { chatId, userId });
    }

    public async Task RemoveUserFromChat(Guid chatId, Guid userId)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            DELETE FROM ChatsUsers
            WHERE ChatId = @chatId AND UserId = @userId
        ", new { chatId, userId });
    }

    public async Task<Chat> Save(Chat chat)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            INSERT OR REPLACE INTO Chats
            (Id, OwnerId, Name, CreatedAt, Version, Type, LastMessageNum)
            VALUES
            (@Id, @OwnerId, @Name, @CreatedAt, @Version, @Type, @LastMessageNum)
        ", chat);

        return chat;
    }

    public async Task<Chat?> UpdateName(Guid chatId, string name)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Chats SET Name = @name WHERE Id = @chatId
        ", new { chatId, name });

        return await Find(chatId);
    }

    public async Task<Chat?> UpdateVersion(Guid chatId, long version)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Chats SET Version = @version WHERE Id = @chatId
        ", new { chatId, version });

        return await Find(chatId);
    }

    public async Task Delete(Guid id)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(
            "DELETE FROM Chats WHERE Id = @id",
            new { id });
    }

    public async Task<Chat?> UpdateLastMessageNum(Guid chatId, ulong lastMessageNum)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Chats 
            SET LastMessageNum = @lastMessageNum 
            WHERE Id = @chatId
        ", new { chatId, lastMessageNum });

        return await Find(chatId);
    }

    public async Task UpdateLastReadMessageNum(Guid chatId, Guid userId, ulong lastReadMessageNum)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE ChatsUsers
            SET LastReadMessageNum = @lastReadMessageNum
            WHERE ChatId = @chatId AND UserId = @userId
        ", new { chatId, userId, lastReadMessageNum });
    }
}