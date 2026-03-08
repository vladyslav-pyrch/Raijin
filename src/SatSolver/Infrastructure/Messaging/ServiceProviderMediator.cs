using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class ServiceProviderMediator(IServiceProvider provider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);
        Type requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        object handler = provider.GetRequiredService(requestHandlerType);

        return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        Type requestHandlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        object handler = provider.GetRequiredService(requestHandlerType);

        return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }
}