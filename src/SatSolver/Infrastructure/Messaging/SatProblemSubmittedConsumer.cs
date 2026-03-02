using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class SatProblemSubmittedConsumer(
    IEnumerable<IMessageHandler<ISatProblemSubmitted>> handlers,
    ILogger<SatProblemSubmittedConsumer> logger
) : IConsumer<ISatProblemSubmitted>
{
    public async Task Consume(ConsumeContext<ISatProblemSubmitted> context)
    {
        List<IMessageHandler<ISatProblemSubmitted>> handlerList = handlers.ToList();

        IEnumerable<Task> tasks = handlerList.Select(h => h.Handle(context.Message, context.CancellationToken));
        await Task.WhenAll(tasks);
    }
}