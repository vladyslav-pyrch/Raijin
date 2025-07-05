namespace Njinx.ProblemSolvingService.Application.Cqrs;

public interface ICommandDispatcher
{
    public Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;

    public Task<TResult> Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult> where TResult : new();
}