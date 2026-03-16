using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class ServiceProviderMediator(IServiceProvider provider) : IMediator
{
    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type requestType = command.GetType();
        Type responseType = typeof(TResponse);
        Type requestHandlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, responseType);

        object handler = provider.GetRequiredService(requestHandlerType);

        return ((dynamic)handler).Handle((dynamic)command, cancellationToken);
    }

    public Task Send(ICommand command, CancellationToken cancellationToken)
    {
        Type requestType = command.GetType();
        Type requestHandlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);

        object handler = provider.GetRequiredService(requestHandlerType);

        return ((dynamic)handler).Handle((dynamic)command, cancellationToken);
    }
}