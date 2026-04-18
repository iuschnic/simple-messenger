using BL.Interfaces;
using Dapper;
using DB.Database;
using BL.Models;

namespace DB.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DbConnectionFactory _factory;

    public MessageRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public Message Find(long id)
    {
        using var db = _factory.Create();

        return db.QueryFirstOrDefault<Message>(
            "SELECT * FROM Messages WHERE MessageNumber = @id",
            new { id });
    }

    public List<Message> FindChatMessages(Guid chatId)
    {
        using var db = _factory.Create();

        return db.Query<Message>(@"
        SELECT * FROM Messages
        WHERE ChatId = @chatId
          AND Deleted = 0
        ORDER BY MessageNumber
    ", new { chatId }).ToList();
    }

    public Message Save(Message message)
    {
        using var db = _factory.Create();

        var id = db.ExecuteScalar<int>(@"
        INSERT INTO Messages
        (ChatId, SenderId, Text, CreatedAt, EditedAt, Deleted, Version, Type)
        VALUES
        (@ChatId, @SenderId, @Text, @CreatedAt, @EditedAt, @Deleted, @Version, @Type);

        SELECT last_insert_rowid();
    ", message);

        message.MessageNumber = id;

        return message;
    }

    public Message Edit(long id, DateTime editedAt, string newText)
    {
        using var db = _factory.Create();

        db.Execute(@"
            UPDATE Messages
            SET Text = @newText,
                EditedAt = @editedAt
            WHERE MessageNumber = @id
        ", new { id, editedAt, newText });

        return Find(id);
    }

    public void Delete(long id)
    {
        using var db = _factory.Create();

        db.Execute(@"
            UPDATE Messages
            SET Deleted = 1
            WHERE MessageNumber = @id
        ", new { id });
    }
    
    public long GetLastMessageNumber(Guid chatId)
    {
        using var db = _factory.Create();

        var result = db.ExecuteScalar<long?>(@"
        SELECT MAX(MessageNumber)
        FROM Messages
        WHERE ChatId = @chatId
    ", new { chatId });

        return result ?? 0;
    }
}