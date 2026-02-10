namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

/// <summary>
/// Background service that processes SAT solver tasks from the queue.
///
/// This service:
/// 1. Dequeues tasks from the background task queue
/// 2. Calls the SAT solver (which may run for hours)
/// 3. Updates the database with the result
/// 4. Handles errors gracefully
///
/// Each task gets its own DbContext scope, preventing disposal issues.
/// </summary>
public class SatSolverBackgroundService(
    IBackgroundSatSolverTaskQueue satSolverTaskQueue,
    IServiceProvider serviceProvider,
    ILogger<SatSolverBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SAT Solver background service is starting");

        try
        {
            // Keep processing tasks until the service is stopped
            await ProcessTasks(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("SAT Solver background service is stopping");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in SAT Solver background service");
            throw;
        }
    }

    private async Task ProcessTasks(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Dequeue the next task (will wait if queue is empty)
                var task = await satSolverTaskQueue.DequeueAsync(stoppingToken);

                // Process the task with its own DbContext scope
                await ProcessTask(task, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Application is shutting down
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing SAT solver task from queue");
                // Continue processing next tasks instead of crashing
            }
        }
    }

    private async Task ProcessTask(SatSolverTask task, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing SAT problem (Id: {SatProblemId})", task.SatProblemId);

            // Create a new scope for this task's database operations
            // This ensures the DbContext is properly created and disposed for this specific task
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SatProblemsDbContext>();
            var satSolver = scope.ServiceProvider.GetRequiredService<ISatSolver>();

            // Fetch the SAT problem from the database
            var satProblem = await dbContext.SatProblems.FindAsync(
                new object?[] { task.SatProblemId },
                cancellationToken: cancellationToken);

            if (satProblem == null)
            {
                logger.LogWarning("SAT problem with ID {SatProblemId} not found in database", task.SatProblemId);
                return;
            }

            try
            {
                // Call the SAT solver (this is the long-running operation)
                logger.LogInformation("Starting solver for SAT problem {SatProblemId}", task.SatProblemId);
                string solverResult = await satSolver.SolveAsync(task.Dimacs, task.Timeout, cancellationToken);

                // Update the problem with the result
                satProblem.Status = SatProblemStatus.Completed;
                satProblem.Result = solverResult;
                satProblem.CompletedAt = DateTime.UtcNow;

                logger.LogInformation("Solver completed for SAT problem {SatProblemId}", task.SatProblemId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Solver failed for SAT problem {SatProblemId}", task.SatProblemId);

                // Update status to indicate failure
                satProblem.Status = SatProblemStatus.Failed;
                satProblem.Result = ex.Message;
                satProblem.CompletedAt = DateTime.UtcNow;
            }

            // Save changes to database with proper error handling
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Saved result for SAT problem {SatProblemId}", task.SatProblemId);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("SAT solver task for problem {SatProblemId} was cancelled", task.SatProblemId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while processing SAT problem {SatProblemId}", task.SatProblemId);
            throw;
        }
    }
}


