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

    public Chat Find(Guid id)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<Chat>(
            "SELECT * FROM Chats WHERE Id = @id",
            new { id });
    }

    public List<Chat> GetAllChats()
    {
        using var db = _factory.Create();
        return db.Query<Chat>(@"
        SELECT *
        FROM Chats
    ").ToList();
    }

    public List<User> FindChatUsers(Guid chatId)
    {
        using var db = _factory.Create();

        return db.Query<User>(@"
            SELECT u.*
            FROM Users u
            JOIN ChatsUsers cu ON cu.UserId = u.Id
            WHERE cu.ChatId = @chatId
        ", new { chatId }).ToList();
    }

    public void AddUserToChat(Guid chatId, Guid userId)
    {
        using var db = _factory.Create();

        db.Execute(@"
            INSERT OR IGNORE INTO ChatsUsers (ChatId, UserId, LastReadMessageNum)
            VALUES (@chatId, @userId, 0)
        ", new { chatId, userId });
    }

    public void RemoveUserFromChat(Guid chatId, Guid userId)
    {
        using var db = _factory.Create();

        db.Execute(@"
            DELETE FROM ChatsUsers
            WHERE ChatId = @chatId AND UserId = @userId
        ", new { chatId, userId });
    }

    public Chat Save(Chat chat)
    {
        using var db = _factory.Create();

        db.Execute(@"
            INSERT OR REPLACE INTO Chats
            (Id, OwnerId, Name, CreatedAt, UpdatedAt, Version, Type, LastMessageNum)
            VALUES
            (@Id, @OwnerId, @Name, @CreatedAt, @UpdatedAt, @Version, @Type, @LastMessageNum)
        ", chat);

        return chat;
    }

    public Chat UpdateName(Guid chatId, string name)
    {
        using var db = _factory.Create();

        db.Execute(@"
            UPDATE Chats SET Name = @name WHERE Id = @chatId
        ", new { chatId, name });

        return Find(chatId);
    }

    public Chat UpdateVersion(Guid chatId, long version)
    {
        using var db = _factory.Create();

        db.Execute(@"
            UPDATE Chats SET Version = @version WHERE Id = @chatId
        ", new { chatId, version });

        return Find(chatId);
    }

    public void Delete(Guid id)
    {
        using var db = _factory.Create();
        db.Execute("DELETE FROM Chats WHERE Id = @id", new { id });
    }
    
    public Chat UpdateLastMessageNum(Guid chatId, long lastMessageNum)
    {
        using var db = _factory.Create();

        db.Execute(@"
        UPDATE Chats 
        SET LastMessageNum = @lastMessageNum 
        WHERE Id = @chatId
    ", new { chatId, lastMessageNum });

        return Find(chatId);
    }
    
    public void UpdateLastReadMessageNum(Guid chatId, Guid userId, long lastReadMessageNum)
    {
        using var db = _factory.Create();

        db.Execute(@"
        UPDATE ChatsUsers
        SET LastReadMessageNum = @lastReadMessageNum
        WHERE ChatId = @chatId AND UserId = @userId
    ", new { chatId, userId, lastReadMessageNum });
    }
    
    
}