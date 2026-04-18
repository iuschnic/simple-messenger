namespace DB.Database;

using Dapper;

public class DbInitializer
{
    private readonly DbConnectionFactory _factory;

    public DbInitializer(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task Reset()
    {
        using var db = _factory.Create();
        await db.OpenAsync();

        var sql = @"
        DELETE FROM ChatsUsers;
        DELETE FROM Messages;
        DELETE FROM Chats;
        DELETE FROM Users;
        DELETE FROM CurrentUser;
    ";

        await db.ExecuteAsync(sql);
    }
    
    public async Task Init()
    {
        using var db = _factory.Create();
        await db.OpenAsync();

        var sql = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id TEXT PRIMARY KEY,
            UniqueName TEXT UNIQUE,
            DisplayName TEXT,
            ContactName TEXT
        );

        CREATE TABLE IF NOT EXISTS CurrentUser (
            Id TEXT PRIMARY KEY,
            UniqueName TEXT NOT NULL,
            Email TEXT,
            PasswordHash TEXT,
            DisplayedName TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Chats (
            Id TEXT PRIMARY KEY,
            OwnerId TEXT,
            Name TEXT,
            CreatedAt TEXT,
            UpdatedAt TEXT,
            Version INTEGER,
            Type TEXT,
            LastMessageNum INTEGER,
            FOREIGN KEY (OwnerId) REFERENCES Users(Id)
        );

        CREATE TABLE IF NOT EXISTS Messages (
            MessageNumber INTEGER PRIMARY KEY AUTOINCREMENT,
            ChatId TEXT,
            SenderId TEXT,
            Text TEXT,
            CreatedAt TEXT,
            EditedAt TEXT,
            Deleted INTEGER,
            Version INTEGER,
            Type TEXT,
            ReplyToMessageNumber INTEGER,
            ForwardedFromUserId TEXT,
            FOREIGN KEY (ChatId) REFERENCES Chats(Id),
            FOREIGN KEY (SenderId) REFERENCES Users(Id),
            FOREIGN KEY (ForwardedFromUserId) REFERENCES Users(Id),
            FOREIGN KEY (ReplyToMessageNumber) REFERENCES Messages(MessageNumber)
        );

        CREATE TABLE IF NOT EXISTS ChatsUsers (
            ChatId TEXT,
            UserId TEXT,
            LastReadMessageNum INTEGER,
            PRIMARY KEY (ChatId, UserId),
            FOREIGN KEY (ChatId) REFERENCES Chats(Id),
            FOREIGN KEY (UserId) REFERENCES Users(Id)
        );

        ";

        await db.ExecuteAsync(sql);
        //await db.ExecuteAsync("PRAGMA foreign_keys = ON;");
    }
}