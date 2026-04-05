using FluentResults;
using MassTransit;
using Quartz;
using Raijin.SatSolver.Application.Features.SatProblems;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Worker.Jobs;

public sealed class SolvePendingProblemsJob(
    ICorrelationContextAccessor correlationContextAccessor,
    IServiceScopeFactory scopeFactory,
    ILogger<SolvePendingProblemsJob> logger
) : IJob
{
    private const int MaxJobs = 3;

    private const int MaxRefire = 5;

    private static readonly SemaphoreSlim Semaphore = new(MaxJobs);

    public static readonly JobKey Key = new("solve-pending-problems-job");

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > MaxRefire)
            return;

        Guid id = NewId.NextGuid();
        correlationContextAccessor.CorrelationContext = new CorrelationContext(id, id, null);

        bool acquired = await Semaphore.WaitAsync(TimeSpan.Zero, context.CancellationToken);
        if (!acquired)
            return;

        _ = SolveAsync(context.CancellationToken);
    }

    private async Task SolveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            ICorrelationContextAccessor localCorrelationContextAccessor =
                scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
            localCorrelationContextAccessor.CorrelationContext = correlationContextAccessor.CorrelationContext;

            using IDisposable? loggingScope = logger.BeginScope(
                "InitiatorId: {InitiatorId}, CorrelationId: {CorrelationId}, UserId: {UserId}",
                localCorrelationContextAccessor.CorrelationContext.InitiatorId,
                localCorrelationContextAccessor.CorrelationContext.CorrelationId,
                localCorrelationContextAccessor.CorrelationContext.UserId
            );

            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            Result result = await mediator.Send(new SolveNextPendingSatProblemCommand(), cancellationToken);
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