namespace LevelUpClone.Application.DTOs;

public sealed class ActivityDto
{
    public int ActivityId { get; init; }
    public string ActivityName { get; init; } = "";
    public int ActivityKind { get; init; }
    public int DefaultPoints { get; init; }
    public bool IsActive { get; init; }
}
