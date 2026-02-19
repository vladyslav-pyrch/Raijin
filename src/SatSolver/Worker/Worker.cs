using Raijin.SatSolver.Infrastructure.Messaging;

namespace Raijin.SatSolver.Worker;

public class Worker(IEnumerable<IConsumer> consumers) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (IConsumer consumer in consumers)
            await consumer.Start(cancellationToken: stoppingToken);
    }
}
