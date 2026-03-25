using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class MessageConsumer<TMessage>(
    IEnumerable<IMessageHandler<TMessage>> handlers,
    ILogger<MessageConsumer<TMessage>> logger
) : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    private string MessageType => typeof(TMessage).Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        List<IMessageHandler<TMessage>> handlerList = handlers.ToList();

        logger.LogDebug("Dispatching {MessageType} to {HandlerCount} handler(s)", MessageType, handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));

        await Task.WhenAll(tasks);
    }
}