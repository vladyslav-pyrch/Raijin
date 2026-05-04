using FluentResults;
using Quartz;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.SatSolver.Jobs;

public sealed class SolvePendingProblemsJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SolvePendingProblemsJob> logger
) : IJob
{
    private static SemaphoreSlim? _semaphore;

    public static readonly JobKey Key = new("solve-pending-problems-job");
    private readonly int _maxJobs = configuration.GetValue<int>("MAX_JOBS_COUNT");

    private readonly int _maxRefire = configuration.GetValue<int>("MAX_REFIRE_COUNT");

    private SemaphoreSlim Semaphore
    {
        get
        {
            _semaphore ??= new SemaphoreSlim(_maxJobs);

            return _semaphore;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > _maxRefire)
            return;

        bool acquired = await Semaphore.WaitAsync(TimeSpan.Zero, context.CancellationToken);
        if (!acquired)
            return;

        _ = SolveAsync(context.CancellationToken)
            .ContinueWith(
                t => logger.LogError(t.Exception, "Unhandled exception in SolveAsync."),
                TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task SolveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            Result result = await mediator.Send(new SolveNextPendingProblemCommand(), cancellationToken);

            if (result.IsFailed)
                logger.LogWarning("Solve pending SAT run returned failure: {Errors}", result.Errors);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occured during solving of a pending job.");
        }
        finally
        {
            Semaphore.Release();
        }
    }
}