namespace Njinx.ProblemSolvingService.Application.Cqrs;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    public Task Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}