using FluentResults;

namespace Raijin.CombinatoricsService.Application.Messaging.Behaviors;

public sealed class ContextBehavior<TRequest, TResponse>(IMessageContextAccessor contextAccessor)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IContextualRequest
{
    public Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result<TResponse>>> next)
    {
        if (request is IContextualRequest contextualRequest)
            contextAccessor.CurrentContext = contextualRequest.Context;
        
        return next();
    }
}

public sealed class ContextBehavior<TRequest>(IMessageContextAccessor contextAccessor)
    : IPipelineBehavior<TRequest> where TRequest : IRequest, IContextualRequest
{
    public Task<Result> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result>> next)
    {
        if (request is IContextualRequest contextualRequest)
            contextAccessor.CurrentContext = contextualRequest.Context;
        
        return next();
    }
}