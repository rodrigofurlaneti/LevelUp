using LevelUpClone.Application.Abstractions;
using LevelUpClone.Domain.Interfaces;

namespace LevelUpClone.Application.Cqrs.Users;

public sealed class UpsertUserCommand : ICommand<int>
{
    public string UserName { get; init; } = "";
    public string DisplayName { get; init; } = "";
}

public sealed class UpsertUserHandler : ICommandHandler<UpsertUserCommand, int>
{
    private readonly IUserRepository _repo;
    public UpsertUserHandler(IUserRepository repo) => _repo = repo;
    public int Handle(UpsertUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.UserName)) throw new ArgumentException("UserName required");
        if (string.IsNullOrWhiteSpace(command.DisplayName)) throw new ArgumentException("DisplayName required");
        return _repo.UpsertUser(command.UserName, command.DisplayName);
    }
}
