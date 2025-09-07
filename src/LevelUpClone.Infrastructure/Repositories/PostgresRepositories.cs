using Dapper;
using LevelUpClone.Domain.Enums;
using LevelUpClone.Domain.Interfaces;
using LevelUpClone.Infrastructure.Persistence;
using LevelUpClone.Infrastructure.Security;
using Npgsql;

namespace LevelUpClone.Infrastructure.Repositories
{
    public sealed class UserRepositoryPg : IUserRepository
    {
        private readonly PostgresConnectionFactory _factory;
        public UserRepositoryPg(PostgresConnectionFactory factory) => _factory = factory;

        public int UpsertUser(string userName, string displayName)
        {
            using var conn = _factory.Open();
            var sql = """
            INSERT INTO "UserAccount" ("UserName", "DisplayName")
            VALUES (@userName, @displayName)
            ON CONFLICT ("UserName")
            DO UPDATE SET "DisplayName" = EXCLUDED."DisplayName", "IsActive" = TRUE
            RETURNING "UserId";
            """;
            return conn.ExecuteScalar<int>(sql, new { userName, displayName });
        }
    }

    public sealed class ActivityRepositoryPg : IActivityRepository
    {
        private readonly PostgresConnectionFactory _factory;
        public ActivityRepositoryPg(PostgresConnectionFactory factory) => _factory = factory;

        public int CreateActivity(int userId, string activityName, int activityKind, int defaultPoints)
        {
            using var conn = _factory.Open();
            var sql = """
            INSERT INTO "Activity" ("UserId", "ActivityName", "ActivityKind", "DefaultPoints")
            VALUES (@userId, @activityName, @activityKind, @defaultPoints)
            RETURNING "ActivityId";
            """;
            return conn.ExecuteScalar<int>(sql, new { userId, activityName, activityKind, defaultPoints });
        }

        public IEnumerable<(int ActivityId, string ActivityName, int ActivityKind, int DefaultPoints, bool IsActive)>
            GetActivities(bool onlyActive)
        {
            using var conn = _factory.Open();
            var sql = """
            SELECT "ActivityId", "ActivityName", "ActivityKind", "DefaultPoints", "IsActive"
            FROM "Activity"
            WHERE @onlyActive = FALSE OR "IsActive" = TRUE;
            """;
            return conn.Query<(int, string, int, int, bool)>(sql, new { onlyActive });
        }

        public void SetActivityActive(int activityId, bool isActive)
        {
            using var conn = _factory.Open();
            var sql = """
            UPDATE "Activity"
               SET "IsActive" = @isActive
             WHERE "ActivityId" = @activityId;
            """;
            conn.Execute(sql, new { activityId, isActive });
        }
    }

    public sealed class ActivityLogRepositoryPg : IActivityLogRepository
    {
        private readonly PostgresConnectionFactory _factory;
        public ActivityLogRepositoryPg(PostgresConnectionFactory factory) => _factory = factory;

        public long Log(int userId, DateOnly activityDate, int? activityId, FundamentalCode? fundamentalCode, int pointsAwarded, string? notesText)
        {
            using var conn = _factory.Open();
            var sql = """
            INSERT INTO "ActivityLog"
                ("UserId", "ActivityId", "FundamentalCode", "ActivityDate", "PointsAwarded", "NotesText")
            VALUES
                (@userId, @activityId, @fundamentalCode, @activityDate, @pointsAwarded, @notesText)
            RETURNING "LogId";
            """;
            try
            {
                return conn.ExecuteScalar<long>(sql, new
                {
                    userId,
                    activityId,
                    fundamentalCode = (int?)fundamentalCode,
                    activityDate = new DateTime(activityDate.Year, activityDate.Month, activityDate.Day, 0, 0, 0, DateTimeKind.Utc),
                    pointsAwarded,
                    notesText
                });
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Registro já existe para esta data (fundamental/atividade).", ex);
            }
        }

        public int GetDailyScore(int userId, DateOnly activityDate)
        {
            using var conn = _factory.Open();
            var sql = """
            SELECT COALESCE(SUM("PointsAwarded"), 0)
              FROM "ActivityLog"
             WHERE "UserId" = @userId
               AND "ActivityDate" = @activityDate;
            """;
            return conn.ExecuteScalar<int>(sql, new
            {
                userId,
                activityDate = new DateTime(activityDate.Year, activityDate.Month, activityDate.Day, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }

    public sealed class UserServicePg : IUserService
    {
        private readonly PostgresConnectionFactory _factory;

        public UserServicePg(PostgresConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> ValidateAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return false;

            using var conn = _factory.Open();
            var row = await conn.QueryFirstOrDefaultAsync<(string? PasswordHash, bool IsActive)>(
                """
            SELECT "PasswordHash", "IsActive"
              FROM "UserAccount"
             WHERE "UserName" = @userName
            """,
                new { userName });

            if (row.PasswordHash is null || !row.IsActive) return false;

            return PasswordHasher.Verify(password, row.PasswordHash);
        }

        public async Task SetPasswordAsync(string userName, string newPassword)
        {
            var hash = PasswordHasher.Hash(newPassword);
            using var conn = _factory.Open();
            await conn.ExecuteAsync(
                """
            UPDATE "UserAccount" SET "PasswordHash" = @hash
             WHERE "UserName" = @userName
            """,
                new { userName, hash });
        }
    }
}


