using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging.Consumers;

public sealed class SatProblemSentConsumer(
    IEnumerable<IMessageHandler<ISatProblemSent>> handlers,
    ILogger<SatProblemSentConsumer> logger
) : IConsumer<ISatProblemSent>
{
    public async Task Consume(ConsumeContext<ISatProblemSent> context)
    {
        logger.LogInformation("Consuming ISatProblemSent for SAT problem {SatProblemId}, MessageId: {MessageId}, CorrelationId: {CorrelationId}, CausationId: {CausationId}",
            context.Message.SatProblemId, context.Message.MessageId, context.Message.CorrelationId, context.Message.CausationId);

        List<IMessageHandler<ISatProblemSent>> handlerList = handlers.ToList();

        logger.LogDebug("Dispatching ISatProblemSent to {HandlerCount} handler(s)", handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        await Task.WhenAll(tasks);

        logger.LogInformation("Finished consuming ISatProblemSent for SAT problem {SatProblemId}", context.Message.SatProblemId);
    }
}

