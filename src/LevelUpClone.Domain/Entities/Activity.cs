using LevelUpClone.Domain.Enums;

namespace LevelUpClone.Domain.Entities;

public sealed class Activity
{
    public int ActivityId { get; private set; }
    public int UserId { get; private set; }
    public string ActivityName { get; private set; }
    public ActivityKind ActivityKind { get; private set; }
    public int DefaultPoints { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public Activity(int activityId, int userId, string activityName, ActivityKind activityKind, int defaultPoints, bool isActive, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(activityName)) throw new ArgumentException("ActivityName required");
        ActivityId = activityId;
        UserId = userId;
        ActivityName = activityName;
        ActivityKind = activityKind;
        DefaultPoints = defaultPoints;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }
}
