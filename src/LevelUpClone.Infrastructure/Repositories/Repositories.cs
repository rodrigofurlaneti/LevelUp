using Dapper;
using LevelUpClone.Domain.Enums;
using LevelUpClone.Domain.Interfaces;
using LevelUpClone.Infrastructure.Persistence;

namespace LevelUpClone.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ConnectionFactory _factory;
    public UserRepository(ConnectionFactory factory) => _factory = factory;
    public int UpsertUser(string userName, string displayName)
    {
        using var conn = _factory.Open();
        return conn.ExecuteScalar<int>("EXEC usp_UserAccount_Upsert @userName, @displayName",
            new { userName, displayName });
    }
}

public sealed class ActivityRepository : IActivityRepository
{
    private readonly ConnectionFactory _factory;
    public ActivityRepository(ConnectionFactory factory) => _factory = factory;

    public int CreateActivity(int userId, string activityName, int activityKind, int defaultPoints)
    {
        using var conn = _factory.Open();
        return conn.ExecuteScalar<int>("EXEC usp_Activity_Create @userId, @activityName, @activityKind, @defaultPoints",
            new { userId, activityName, activityKind, defaultPoints });
    }

    public IEnumerable<(int ActivityId, string ActivityName, int ActivityKind, int DefaultPoints, bool IsActive)> GetActivities(bool onlyActive)
    {
        using var conn = _factory.Open();
        return conn.Query<(int, string, int, int, bool)>("SELECT ActivityId, ActivityName, ActivityKind, DefaultPoints, IsActive FROM Activity WHERE @onlyActive = 0 OR IsActive = 1", new { onlyActive });
    }

    public void SetActivityActive(int activityId, bool isActive)
    {
        using var conn = _factory.Open();
        conn.Execute("UPDATE Activity SET IsActive = @isActive WHERE ActivityId = @activityId", new { activityId, isActive });
    }
}

public sealed class ActivityLogRepository : IActivityLogRepository
{
    private readonly ConnectionFactory _factory;
    public ActivityLogRepository(ConnectionFactory factory) => _factory = factory;

    public long Log(int userId, DateOnly activityDate, int? activityId, FundamentalCode? fundamentalCode, int pointsAwarded, string? notesText)
    {
        using var conn = _factory.Open();
        return conn.ExecuteScalar<long>("EXEC usp_ActivityLog_Log @userId, @activityDate, @activityId, @fundamentalCode, @pointsAwarded, @notesText",
            new
            {
                userId,
                activityDate = activityDate.ToDateTime(TimeOnly.MinValue),
                activityId,
                fundamentalCode,
                pointsAwarded,
                notesText
            });
    }

    public int GetDailyScore(int userId, DateOnly activityDate)
    {
        using var conn = _factory.Open();
        return conn.ExecuteScalar<int>("EXEC usp_Score_GetDaily @userId, @activityDate",
            new { userId, activityDate = activityDate.ToDateTime(TimeOnly.MinValue) });
    }
}
