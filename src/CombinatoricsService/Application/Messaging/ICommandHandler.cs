namespace Raijin.CombinatoricsService.Application.Messaging;

public interface ICommandHandler<in TRequest> where TRequest : ICommand
{
    public Task Handle(TRequest command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TRequest, TResponse> where TRequest : ICommand<TResponse>
{
    public Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);
}