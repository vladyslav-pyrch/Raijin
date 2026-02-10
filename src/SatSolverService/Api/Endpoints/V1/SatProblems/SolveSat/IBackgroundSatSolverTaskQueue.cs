namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

/// <summary>
/// Interface for queueing background SAT solver tasks.
/// </summary>
public interface IBackgroundSatSolverTaskQueue
{
    /// <summary>
    /// Enqueues a SAT solver task for background processing.
    /// </summary>
    ValueTask EnqueueAsync(SatSolverTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues a SAT solver task for processing. Waits if queue is empty.
    /// </summary>
    ValueTask<SatSolverTask> DequeueAsync(CancellationToken cancellationToken = default);
}

