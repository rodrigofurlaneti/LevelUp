using LevelUpClone.Application.Abstractions;
using LevelUpClone.Domain.Interfaces;

namespace LevelUpClone.Application.Cqrs.Activities;

public sealed class CreateActivityCommand : ICommand<int>
{
    public int UserId { get; init; }
    public string ActivityName { get; init; } = "";
    public int ActivityKind { get; init; } // 1 Task, 2 Negative
    public int DefaultPoints { get; init; } // calculated outside
}

public sealed class CreateActivityHandler : ICommandHandler<CreateActivityCommand, int>
{
    private readonly IActivityRepository _repo;
    public CreateActivityHandler(IActivityRepository repo) => _repo = repo;
    public int Handle(CreateActivityCommand c)
    {
        if (string.IsNullOrWhiteSpace(c.ActivityName)) throw new ArgumentException("ActivityName required");
        return _repo.CreateActivity(c.UserId, c.ActivityName, c.ActivityKind, c.DefaultPoints);
    }
}
