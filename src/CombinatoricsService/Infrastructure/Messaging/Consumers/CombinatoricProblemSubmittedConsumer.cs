using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging.Consumers;

public sealed class CombinatoricProblemSubmittedConsumer(
    IEnumerable<IMessageHandler<ICombinatoricProblemSubmitted>> handlers,
    ILogger<CombinatoricProblemSubmittedConsumer> logger
) : IConsumer<ICombinatoricProblemSubmitted>
{
    public async Task Consume(ConsumeContext<ICombinatoricProblemSubmitted> context)
    {
        logger.LogInformation("Consuming ICombinatoricProblemSubmitted for problem {CombinatoricProblemId}, MessageId: {MessageId}, CorrelationId: {CorrelationId}, CausationId: {CausationId}",
            context.Message.CombinatoricProblemId, context.Message.MessageId, context.Message.CorrelationId, context.Message.CausationId);

        List<IMessageHandler<ICombinatoricProblemSubmitted>> handlerList = handlers.ToList();

        logger.LogDebug("Dispatching ICombinatoricProblemSubmitted to {HandlerCount} handler(s)", handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        await Task.WhenAll(tasks);

        logger.LogInformation("Finished consuming ICombinatoricProblemSubmitted for problem {CombinatoricProblemId}", context.Message.CombinatoricProblemId);
    }
}
