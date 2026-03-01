using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class SatProblemSubmittedConsumer(
    IEnumerable<IEventHandler<ISatProblemSubmitted>> handlers,
    ILogger<SatProblemSubmittedConsumer> logger
) : IConsumer<ISatProblemSubmitted>
{
    public async Task Consume(ConsumeContext<ISatProblemSubmitted> context)
    {
        logger.LogInformation("Received SatProblemSubmitted event do pice with content: {@MessageContent}",
            context.Message);

        List<IEventHandler<ISatProblemSubmitted>> handlerList = handlers.ToList();
        logger.LogInformation("Dispatching SatProblemSubmitted event to {HandlerCount} handlers", handlerList.Count);

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        await Task.WhenAll(tasks);
    }
}