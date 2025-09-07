using LevelUpClone.Domain.Enums;

namespace LevelUpClone.Domain.Entities;

public sealed class ActivityLog
{
    public long LogId { get; private set; }
    public int UserId { get; private set; }
    public int? ActivityId { get; private set; }
    public FundamentalCode? FundamentalCode { get; private set; }
    public DateOnly ActivityDate { get; private set; }
    public int PointsAwarded { get; private set; }
    public string? NotesText { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public ActivityLog(long logId, int userId, int? activityId, FundamentalCode? fundamentalCode, DateOnly activityDate, int pointsAwarded, string? notesText, DateTime createdAtUtc)
    {
        LogId = logId;
        UserId = userId;
        ActivityId = activityId;
        FundamentalCode = fundamentalCode;
        ActivityDate = activityDate;
        PointsAwarded = pointsAwarded;
        NotesText = notesText;
        CreatedAtUtc = createdAtUtc;
    }
}
