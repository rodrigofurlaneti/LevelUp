using System.Data;

namespace LevelUpClone.Infrastructure.Persistence;

public sealed class UnitOfWork
{
    private readonly IDbConnection _conn;
    private IDbTransaction? _tx;
    public UnitOfWork(IDbConnection conn) => _conn = conn;
    public void Begin() => _tx = _conn.BeginTransaction();
    public void Commit() { _tx?.Commit(); _tx = null; }
    public void Rollback() { _tx?.Rollback(); _tx = null; }
    public IDbTransaction? Transaction => _tx;
}
