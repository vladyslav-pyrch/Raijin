using FluentResults;

namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IPipelineBehavior<in TRequest, TResponse>
     where TRequest : IRequest<TResponse>
{
     public Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result<TResponse>>> next);
}

public interface IPipelineBehavior<in TRequest>
     where TRequest : IRequest
{
     public Task<Result> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result>> next);
}