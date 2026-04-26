namespace DB.Database;

using Microsoft.Data.Sqlite;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string dbPath)
    {
        _connectionString = $"Data Source={dbPath};Pooling=false";
    }
    public SqliteConnection Create()
        => new SqliteConnection(_connectionString);
}