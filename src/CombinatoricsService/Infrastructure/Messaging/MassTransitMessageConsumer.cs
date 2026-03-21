using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using MessageContext = Raijin.CombinatoricsService.Application.Messaging.MessageContext;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class MassTransitMessageConsumer<TMessage>(
    IEnumerable<IMessageHandler<TMessage>> handlers,
    ILogger<MassTransitMessageConsumer<TMessage>> logger
) : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    private string MessageType => typeof(TMessage).Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageContext = new MessageContext(context.Message);
        logger.LogInformation("Consuming {TMessage}: {@Message}", MessageType, context.Message);

        List<IMessageHandler<TMessage>> handlerList = handlers.ToList();

        logger.LogDebug("Dispatching {TMessage} to {HandlerCount} handler(s)", MessageType, handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while handling {TMessage} with the context {@Context}",
                MessageType, messageContext);
            throw;
        }

        logger.LogInformation("Finished consuming {TMessage} with the context {@Context}", MessageType, messageContext);
    }
}