using Npgsql;
using System.Data;

namespace LevelUpClone.Infrastructure.Persistence;

public sealed class PostgresConnectionFactory
{
    private readonly string _cs;
    public PostgresConnectionFactory(string cs) => _cs = cs;
    public IDbConnection Open()
    {
        var conn = new NpgsqlConnection(_cs);
        conn.Open();
        return conn;
    }
}
