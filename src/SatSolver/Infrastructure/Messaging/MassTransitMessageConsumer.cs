using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class MassTransitMessageConsumer<TMessage>(
    IEnumerable<IMessageHandler<TMessage>> handlers,
    ILogger<MassTransitMessageConsumer<TMessage>> logger
) : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageContext = new Application.Messaging.MessageContext(context.Message);
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

    private string MessageType => typeof(TMessage).Name;
}