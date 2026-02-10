using System.Threading.Channels;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

/// <summary>
/// Background task queue implementation using System.Threading.Channels.
/// This is thread-safe and efficient for inter-thread communication.
/// </summary>
public class BackgroundSatSolverTaskQueue : IBackgroundSatSolverTaskQueue
{
    private readonly Channel<SatSolverTask> _queue;

    public BackgroundSatSolverTaskQueue(int maxQueueSize = 100)
    {
        // Bounded channel with max queue size to prevent unbounded memory growth
        var options = new BoundedChannelOptions(maxQueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<SatSolverTask>(options);
    }

    public async ValueTask EnqueueAsync(SatSolverTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        // Write to the channel, waiting if queue is full
        await _queue.Writer.WriteAsync(task, cancellationToken);
    }

    public async ValueTask<SatSolverTask> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // Read from the channel, waiting if queue is empty
        var task = await _queue.Reader.ReadAsync(cancellationToken);
        return task;
    }
}

