using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public class ServiceProviderMediator(IServiceProvider provider) : IMediator
{
    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type commandType = command.GetType();
        Type responseType = typeof(TResponse);
        Type commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, responseType);

        object handler = provider.GetRequiredService(commandHandlerType);

        return ((dynamic)handler).Handle((dynamic)command, cancellationToken);
    }

    public Task Send(ICommand command, CancellationToken cancellationToken)
    {
        Type commandType = command.GetType();
        Type commandHandlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);

        object handler = provider.GetRequiredService(commandHandlerType);

        return ((dynamic)handler).Handle((dynamic)command, cancellationToken);
    }
}