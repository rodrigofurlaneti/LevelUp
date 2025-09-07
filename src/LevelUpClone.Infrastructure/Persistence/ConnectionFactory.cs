using Microsoft.Data.SqlClient;
using System.Data;

namespace LevelUpClone.Infrastructure.Persistence;

public sealed class ConnectionFactory
{
    private readonly string _cs;
    public ConnectionFactory(string cs) => _cs = cs;
    public IDbConnection Open()
    {
        var conn = new SqlConnection(_cs);
        conn.Open();
        return conn;
    }
}
