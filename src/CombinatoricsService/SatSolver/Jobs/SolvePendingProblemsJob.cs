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
        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobKey"] = context.JobDetail.Key.ToString()
        });

        logger.LogDebug("SAT solve job triggered. JobKey={JobKey}", context.JobDetail.Key);

        bool acquired = await Semaphore.WaitAsync(TimeSpan.Zero, context.CancellationToken);
        if (!acquired)
        {
            logger.LogInformation(
                "SAT solve job skipped because concurrency limit is reached. JobKey={JobKey}",
                context.JobDetail.Key);
            return;
        }

        logger.LogDebug("SAT solve job acquired worker slot. JobKey={JobKey}", context.JobDetail.Key);

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
            else
                logger.LogDebug("SAT solve job completed. Outcome={Outcome}", "Success");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurred during solving of a pending job.");
        }
        finally
        {
            Semaphore.Release();
            logger.LogDebug("SAT solve job released worker slot.");
        }
    }
}
