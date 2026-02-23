namespace Raijin.SatSolver.Application.Cqrs;

public interface IMediator
{
    public Task Send(IRequest request, CancellationToken cancellationToken);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}