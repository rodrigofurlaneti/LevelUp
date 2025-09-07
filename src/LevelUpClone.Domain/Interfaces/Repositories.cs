using LevelUpClone.Domain.Enums;

namespace LevelUpClone.Domain.Interfaces;

public interface IUserRepository
{
    int UpsertUser(string userName, string displayName);
}

public interface IActivityRepository
{
    int CreateActivity(int userId, string activityName, int activityKind, int defaultPoints);
    IEnumerable<(int ActivityId, string ActivityName, int ActivityKind, int DefaultPoints, bool IsActive)> GetActivities(bool onlyActive);
    void SetActivityActive(int activityId, bool isActive);
}

public interface IActivityLogRepository
{
    long Log(int userId, DateOnly activityDate, int? activityId, FundamentalCode? fundamentalCode, int pointsAwarded, string? notesText);
    int GetDailyScore(int userId, DateOnly activityDate);
}

public interface IUnitOfWork
{
    void Begin();
    void Commit();
    void Rollback();
}
