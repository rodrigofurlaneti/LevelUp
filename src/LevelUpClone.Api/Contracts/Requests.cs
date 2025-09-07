namespace LevelUpClone.Api.Contracts;

public sealed class UpsertUserRequest { public string UserName { get; set; } = ""; public string DisplayName { get; set; } = ""; }
public sealed class CreateActivityRequest { public int UserId { get; set; } public string ActivityName { get; set; } = ""; public string ActivityKind { get; set; } = "Task"; }
public sealed class LogFundamentalRequest { public int UserId { get; set; } public string FundamentalCode { get; set; } = ""; public string ActivityDate { get; set; } = ""; public string? NotesText { get; set; } }
public sealed class LogCustomRequest { public int UserId { get; set; } public int ActivityId { get; set; } public string ActivityDate { get; set; } = ""; public string? NotesText { get; set; } }
public sealed class LoginRequest { public string UserName { get; set; } = ""; public string Password { get; set; } = ""; }
