using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging.Consumers;

public sealed class SatProblemSubmittedConsumer(
    IEnumerable<IMessageHandler<ISatProblemSubmitted>> handlers,
    ILogger<SatProblemSubmittedConsumer> logger
) : IConsumer<ISatProblemSubmitted>
{
    public async Task Consume(ConsumeContext<ISatProblemSubmitted> context)
    {
        logger.LogInformation("Consuming ISatProblemSubmitted for SAT problem {SatProblemId}, MessageId: {MessageId}, CorrelationId: {CorrelationId}, CausationId: {CausationId}",
            context.Message.SatProblemId, context.Message.MessageId, context.Message.CorrelationId, context.Message.CausationId);

        List<IMessageHandler<ISatProblemSubmitted>> handlerList = handlers.ToList();

        logger.LogDebug("Dispatching ISatProblemSubmitted to {HandlerCount} handler(s)", handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        await Task.WhenAll(tasks);

        logger.LogInformation("Finished consuming ISatProblemSubmitted for SAT problem {SatProblemId}", context.Message.SatProblemId);
    }
}