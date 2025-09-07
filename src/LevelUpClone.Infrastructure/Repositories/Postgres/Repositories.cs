using Dapper;
using LevelUpClone.Application.Abstractions;   // <- IUserService aqui
using LevelUpClone.Domain.Entities;
using LevelUpClone.Domain.Enums;
using LevelUpClone.Domain.Interfaces;
using LevelUpClone.Infrastructure.Persistence;
using LevelUpClone.Infrastructure.Security;
using Npgsql;
using System.Text.Json;

namespace LevelUpClone.Infrastructure.Repositories.Postgres
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
        public UserServicePg(PostgresConnectionFactory factory) => _factory = factory;

        public async Task<bool> ValidateAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return false;

            using var conn = _factory.Open();
            var row = await conn.QueryFirstOrDefaultAsync<(string? PasswordHash, bool IsActive)>(
                """
                SELECT "PasswordHash", "IsActive"
                  FROM "UserAccount"
                 WHERE lower("UserName") = lower(@userName)
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
                 WHERE lower("UserName") = lower(@userName)
                """,
                new { userName, hash });
        }

        public sealed class GeoClientLogRepositoryPg(PostgresConnectionFactory factory) : IGeoClientLogRepository
        {
            private readonly PostgresConnectionFactory _factory = factory;

            public async Task<long> InsertAsync(GeoClientLogEntry e)
            {
                const string sql = """
        INSERT INTO "GeoClientLog"
        (
          "UserAccountId","RemoteIp","ForwardedFor","UserAgent",
          "Latitude","Longitude","AccuracyMeters","AltitudeMeters","AltitudeAccuracyMeters",
          "SpeedMetersPerSecond","HeadingDegrees","TimestampEpochMs",
          "City","EnrichGranularity","EnrichCountry","EnrichUF","EnrichMunicipio",
          "EnrichCodigoMunicipioIBGE","EnrichBairro","EnrichLogradouro","EnrichNumero",
          "EnrichCEP","EnrichTimezoneId","EnrichConfidence","EnrichSourcesJson","EnrichAttribution",
          "EnvBrowser","EnvBrowserVersion","EnvOperatingSystem","EnvOSVersion","EnvArchitecture",
          "EnvDeviceType","EnvDeviceModel","EnvTouchPoints","EnvIsBot","EnvBotName","EnvLanguage",
          "EnvLanguagesJson","EnvPlatform","EnvIsOnline","EnvTimeZone","EnvScreenWidth","EnvScreenHeight",
          "EnvDevicePixelRatio","EnvReferrer","EnvPageUrl",
          "NetDownlink","NetEffectiveType","NetRtt","NetSaveData",
          "Error","CorrelationId","SessionId"
        )
        VALUES
        (
          @UserAccountId,@RemoteIp,@ForwardedFor,@UserAgent,
          @Latitude,@Longitude,@AccuracyMeters,@AltitudeMeters,@AltitudeAccuracyMeters,
          @SpeedMetersPerSecond,@HeadingDegrees,@TimestampEpochMs,
          @City,@EnrichGranularity,@EnrichCountry,@EnrichUF,@EnrichMunicipio,
          @EnrichCodigoMunicipioIBGE,@EnrichBairro,@EnrichLogradouro,@EnrichNumero,
          @EnrichCEP,@EnrichTimezoneId,@EnrichConfidence,
          CAST(@EnrichSourcesJson AS jsonb),@EnrichAttribution,
          @EnvBrowser,@EnvBrowserVersion,@EnvOperatingSystem,@EnvOSVersion,@EnvArchitecture,
          @EnvDeviceType,@EnvDeviceModel,@EnvTouchPoints,@EnvIsBot,@EnvBotName,@EnvLanguage,
          CAST(@EnvLanguagesJson AS jsonb),@EnvPlatform,@EnvIsOnline,@EnvTimeZone,@EnvScreenWidth,@EnvScreenHeight,
          @EnvDevicePixelRatio,@EnvReferrer,@EnvPageUrl,
          @NetDownlink,@NetEffectiveType,@NetRtt,@NetSaveData,
          @Error,@CorrelationId,@SessionId
        )
        RETURNING "Id";
        """;

                await using var conn = (NpgsqlConnection)_factory.Open();
                var id = await conn.ExecuteScalarAsync<long>(sql, new
                {
                    e.UserAccountId,
                    e.RemoteIp,
                    e.ForwardedFor,
                    e.UserAgent,
                    e.Latitude,
                    e.Longitude,
                    e.AccuracyMeters,
                    e.AltitudeMeters,
                    e.AltitudeAccuracyMeters,
                    e.SpeedMetersPerSecond,
                    e.HeadingDegrees,
                    e.TimestampEpochMs,
                    e.City,
                    e.EnrichGranularity,
                    e.EnrichCountry,
                    e.EnrichUF,
                    e.EnrichMunicipio,
                    e.EnrichCodigoMunicipioIBGE,
                    e.EnrichBairro,
                    e.EnrichLogradouro,
                    e.EnrichNumero,
                    e.EnrichCEP,
                    e.EnrichTimezoneId,
                    e.EnrichConfidence,
                    EnrichSourcesJson = e.EnrichSourcesJson is null ? null : JsonSerializer.Serialize(e.EnrichSourcesJson),
                    e.EnrichAttribution,
                    e.EnvBrowser,
                    e.EnvBrowserVersion,
                    e.EnvOperatingSystem,
                    e.EnvOSVersion,
                    e.EnvArchitecture,
                    e.EnvDeviceType,
                    e.EnvDeviceModel,
                    e.EnvTouchPoints,
                    e.EnvIsBot,
                    e.EnvBotName,
                    e.EnvLanguage,
                    EnvLanguagesJson = e.EnvLanguagesJson is null ? null : JsonSerializer.Serialize(e.EnvLanguagesJson),
                    e.EnvPlatform,
                    e.EnvIsOnline,
                    e.EnvTimeZone,
                    e.EnvScreenWidth,
                    e.EnvScreenHeight,
                    e.EnvDevicePixelRatio,
                    e.EnvReferrer,
                    e.EnvPageUrl,
                    e.NetDownlink,
                    e.NetEffectiveType,
                    e.NetRtt,
                    e.NetSaveData,
                    e.Error,
                    e.CorrelationId,
                    e.SessionId
                });
                return id;
            }
        }
    }
}
