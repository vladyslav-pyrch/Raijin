using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class ServiceProviderMediator(IServiceProvider provider) : IMediator
{
    public Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);
        Type requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        object handler = provider.GetRequiredService(requestHandlerType);

        Func<Task<Result<TResponse>>> handlerDelegate = () => ((dynamic)handler).Handle((dynamic)request, cancellationToken);
        
        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        IEnumerable<object> behaviors = provider.GetServices(behaviorType)
            .Where(service => service is not null)
            .Cast<object>()
            .Reverse();
        
        Func<Task<Result<TResponse>>> pipeline = behaviors.Aggregate(handlerDelegate, (next, behavior) =>
        {
            return () => ((dynamic)behavior).Handle((dynamic)request, cancellationToken, next);
        });

        return pipeline();
    }

    public Task<Result> Send(IRequest request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        Type requestHandlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        object handler = provider.GetRequiredService(requestHandlerType);

        Func<Task<Result>> handlerDelegate = () => ((dynamic)handler).Handle((dynamic)request, cancellationToken);
        
        Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(requestType);
        IEnumerable<object> behaviors = provider.GetServices(behaviorType)
            .Where(service => service is not null)
            .Cast<object>()
            .Reverse();
        
        Func<Task<Result>> pipeline = behaviors.Aggregate(handlerDelegate, (next, behavior) =>
        {
            return () => ((dynamic)behavior).Handle((dynamic)request, cancellationToken, next);
        });
        
        return pipeline();
    }
}