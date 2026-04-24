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

    public async Task<Message?> Find(ulong id)
    {
        using var db = _factory.Create();

        return await db.QueryFirstOrDefaultAsync<Message>(
            "SELECT * FROM Messages WHERE MessageNumber = @id",
            new { id });
    }

    public async Task<List<Message>> FindChatMessages(Guid chatId)
    {
        using var db = _factory.Create();

        var result = await db.QueryAsync<Message>(@"
            SELECT * FROM Messages
            WHERE ChatId = @chatId
              AND Deleted = 0
            ORDER BY MessageNumber
        ", new { chatId });

        return result.ToList();
    }

    public async Task<Message> Save(Message message)
    {
        using var db = _factory.Create();

        var id = await db.ExecuteScalarAsync<long>(@"
            INSERT INTO Messages
            (ChatId, SenderId, Text, CreatedAt, EditedAt, Deleted, Version, Type)
            VALUES
            (@ChatId, @SenderId, @Text, @CreatedAt, @EditedAt, @Deleted, @Version, @Type);

            SELECT last_insert_rowid();
        ", message);

        message.MessageNumber = (ulong)id;

        return message;
    }

    public async Task<Message?> Edit(long id, DateTime editedAt, string newText)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Messages
            SET Text = @newText,
                EditedAt = @editedAt
            WHERE MessageNumber = @id
        ", new { id, editedAt, newText });

        return await Find((ulong)id);
    }

    public async Task Delete(long id)
    {
        using var db = _factory.Create();

        await db.ExecuteAsync(@"
            UPDATE Messages
            SET Deleted = 1
            WHERE MessageNumber = @id
        ", new { id });
    }

    public async Task<long> GetLastMessageNumber(Guid chatId)
    {
        using var db = _factory.Create();

        var result = await db.ExecuteScalarAsync<long?>(@"
            SELECT MAX(MessageNumber)
            FROM Messages
            WHERE ChatId = @chatId
        ", new { chatId });

        return result ?? 0;
    }
}