namespace LevelUpClone.Domain.Entities;

public sealed class UserAccount
{
    public int UserId { get; private set; }
    public string UserName { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public UserAccount(int userId, string userName, string displayName, bool isActive, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("UserName required");
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("DisplayName required");
        UserId = userId;
        UserName = userName;
        DisplayName = displayName;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }
}
