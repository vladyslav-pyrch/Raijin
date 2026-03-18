using FluentResults;

namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task<Result> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}