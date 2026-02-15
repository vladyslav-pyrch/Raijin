using Raijin.SatSolver.Application.Features.SolveSat;

namespace Raijin.SatSolver.Worker;

public class Worker(IServiceProvider provider, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int maxConcurrency = 10;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var runningTasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await semaphore.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            Task task = Task.Run(async () =>
            {
                try
                {
                    using IServiceScope scope = provider.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SolveSatHandler>();

                    await handler.Handle(new SolveSatCommand("p cnf 3 2\n1 -3 0\n-1 2 3 0"), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellations during shutdown.
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to solve SAT problem");
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    semaphore.Release();
                }
            }, stoppingToken);

            runningTasks.Add(task);
            runningTasks.RemoveAll(t => t.IsCompleted);

            await Task.Delay(5000, stoppingToken);
        }

        if (runningTasks.Count > 0)
        {
            await Task.WhenAll(runningTasks);
        }
    }
}
