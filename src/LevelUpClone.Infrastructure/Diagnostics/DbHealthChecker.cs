using Dapper;
using LevelUpClone.Infrastructure.Persistence;
using Npgsql;

namespace LevelUpClone.Infrastructure.Diagnostics
{
    public sealed class DbHealthChecker(PostgresConnectionFactory factory)
    {
        private readonly PostgresConnectionFactory _factory = factory;

        /// <summary>Retorna (ok, errorMessage). ok=true quando SELECT 1 funciona.</summary>
        public async Task<(bool ok, string? error)> CheckAsync(CancellationToken ct = default)
        {
            try
            {
                await using var conn = (NpgsqlConnection)_factory.Open();
                var result = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", cancellationToken: ct));
                return (result == 1, null);
            }
            catch (Exception ex)
            {
                return (false, $"{ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
