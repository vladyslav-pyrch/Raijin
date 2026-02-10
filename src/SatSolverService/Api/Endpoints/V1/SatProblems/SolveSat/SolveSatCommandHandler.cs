using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

public partial class SolveSatCommandHandler(
    SatProblemsDbContext dbContext,
    IBackgroundSatSolverTaskQueue satSolverTaskQueue,
    ILogger<SolveSatCommandHandler> logger)
{
    public async Task<Result<int>> Handle(SolveSatCommand command, CancellationToken cancellationToken)
    {
        string[] lines = command.Dimacs.Split('\n');

        if (NotEnoughLines(lines) || BadFormatting(lines))
            return Result.Fail<int>("Invalid dimacs formatting.");

        try
        {
            // Create a new SatProblem entity in the database
            var satProblem = new SatProblem
            {
                Dimacs = command.Dimacs,
                Status = SatProblemStatus.Solving,
                CreatedAt = DateTime.UtcNow
            };

            // Add to database context
            await dbContext.SatProblems.AddAsync(satProblem, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created SAT problem with ID {SatProblemId}. Enqueueing for background processing",
                satProblem.Id);

            // Enqueue the solver task for background processing
            // This returns immediately without waiting for the solver to complete
            await satSolverTaskQueue.EnqueueAsync(
                new SatSolverTask(satProblem.Id, command.Dimacs, null),
                cancellationToken);

            logger.LogInformation("SAT problem {SatProblemId} enqueued for solving", satProblem.Id);

            // Return the ID to the client immediately
            return Result.Ok(satProblem.Id);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error while creating SAT problem");
            return Result.Fail<int>("Failed to create SAT problem");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while handling SAT problem");
            return Result.Fail<int>("An unexpected error occurred");
        }
    }

    private bool NotEnoughLines(string[] lines) => lines.Length < 2;

    private bool BadFormatting(string[] lines) => !DimacsFirstLineRegex().IsMatch(lines[0]) ||
                                                  lines.Skip(1).Any(clause => !DimacsClauseRegex().IsMatch(clause));

    [GeneratedRegex(@"^p\s+cnf\s+[1-9]\d*\s+[1-9]\d*$")]
    private static partial Regex DimacsFirstLineRegex();

    [GeneratedRegex(@"^(?:-?[1-9]\d*\s+)*0$")]
    private static partial Regex DimacsClauseRegex();
}