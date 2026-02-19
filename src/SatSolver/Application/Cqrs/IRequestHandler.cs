namespace Raijin.SatSolver.Application.Cqrs;

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task Handle(TRequest command, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);
}