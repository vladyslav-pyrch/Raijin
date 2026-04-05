using MassTransit;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class MassTransitMessageBus(
    IPublishEndpoint publishEndpoint,
    ISendEndpointProvider sendEndpointProvider,
    IEndpointNameFormatter endpointNameFormatter
) : IMessageBus
{
    public Task Publish<TMessage>(object message, CancellationToken cancellationToken)
        where TMessage : class, IMessage => publishEndpoint.Publish<TMessage>(message, cancellationToken);

    public async Task Send<TMessage>(object message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        string endpointName = endpointNameFormatter.Message<TMessage>();
        ISendEndpoint endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{endpointName}"));
        await endpoint.Send<TMessage>(message, cancellationToken);
    }
}