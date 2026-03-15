namespace Raijin.SatSolver.Application.Messaging;

public interface IMediator
{
    public Task Send(ICommand command, CancellationToken cancellationToken);

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken);
}