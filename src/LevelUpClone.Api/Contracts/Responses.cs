namespace LevelUpClone.Api.Contracts.Responses;

public sealed class IdResponse { public long Id { get; set; } }
public sealed class DailyScoreResponse { public int TotalPoints { get; set; } }
public sealed class ActivityResponse { public int ActivityId { get; set; } public string ActivityName { get; set; } = ""; public int ActivityKind { get; set; } public int DefaultPoints { get; set; } public bool IsActive { get; set; } }
public sealed class LoginResponse { public string Token { get; set; } = ""; public string DisplayName { get; set; } = ""; }
