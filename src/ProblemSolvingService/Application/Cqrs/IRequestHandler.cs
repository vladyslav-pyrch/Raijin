namespace Raijin.ProblemSolvingService.Application.Cqrs;

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}