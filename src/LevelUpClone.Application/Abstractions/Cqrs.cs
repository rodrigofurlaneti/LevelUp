using System;
using System.Diagnostics.CodeAnalysis;

namespace LevelUpClone.Application.Abstractions;

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "Marker interface: TResult is used by handler constraints and dispatcher binding.")]
public interface ICommand<out TResult> { }

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "Marker interface: TResult is used by handler constraints and dispatcher binding.")]
public interface IQuery<out TResult> { }

// Handlers com variância
public interface ICommandHandler<in TCommand, out TResult> where TCommand : ICommand<TResult>
{
    TResult Handle(TCommand command);
}

public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query);
}

public interface IDispatcher
{
    TResult Send<TResult>(ICommand<TResult> command);
    TResult Query<TResult>(IQuery<TResult> query);
}

public sealed class CommandDispatcher : IDispatcher
{
    private readonly IServiceProvider _sp;
    public CommandDispatcher(IServiceProvider sp) => _sp = sp;

    public TResult Send<TResult>(ICommand<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _sp.GetService(handlerType) ?? throw new InvalidOperationException($"Handler not found for {command.GetType().Name}");
        return handler.Handle((dynamic)command);
    }

    public TResult Query<TResult>(IQuery<TResult> query)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = _sp.GetService(handlerType) ?? throw new InvalidOperationException($"Handler not found for {query.GetType().Name}");
        return handler.Handle((dynamic)query);
    }
}
