using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record SolveNextPendingProblemCommand : IRequest;

public sealed class SolveNextPendingProblemHandler(
    IProblemRepository problemRepository,
    IEnumerable<ISatSolver> satSolvers,
    IUnitOfWork unitOfWork,
    ILogger<SolveNextPendingProblemHandler> logger
) : IRequestHandler<SolveNextPendingProblemCommand>
{
    private readonly IReadOnlyList<ISatSolver> _satSolvers = satSolvers.ToList();

    public async Task<Result> Handle(SolveNextPendingProblemCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransaction(cancellationToken);

        Problem? problem = await problemRepository.GetOldestPendingWithLock(cancellationToken);

        if (problem is null)
        {
            await unitOfWork.Commit(cancellationToken);
            logger.LogDebug("No pending problem available for solving.");
            return Result.Ok();
        }

        problem.ReduceToSat();
        problem.MarkAsRunning();
        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        using IDisposable? problemScope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["ProblemId"] = problem.Id,
            ["Solver"] = problem.Solver
        });

        logger.LogInformation("Problem claimed for solving. ProblemId={ProblemId}", problem.Id);

        ISatSolver? satSolver = problem.Solver is not null
            ? _satSolvers.FirstOrDefault(s => s.Name.Equals(problem.Solver, StringComparison.OrdinalIgnoreCase))
            : _satSolvers.FirstOrDefault();
        
        if (satSolver is null)
        {
            logger.LogError(
                "Requested solver is not registered. ProblemId={ProblemId} Solver={Solver}",
                problem.Id,
                problem.Solver);
            problem.Fail(TimeSpan.FromSeconds(0));
            await problemRepository.Update(problem, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result.Fail($"SAT solver '{problem.Solver}' is not registered.");
        }

        logger.LogInformation(
            "Solver selected for problem. ProblemId={ProblemId} Solver={Solver}",
            problem.Id,
            satSolver.Name);
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            Result<SatSolverResult> solveResult = await satSolver.Solve(problem.SatEncoding!, cancellationToken);
            
            sw.Stop(); 
            
            if (solveResult.IsSuccess)
            {
                problem.Complete(solveResult.Value.Satisfiability, solveResult.Value.Assignment, sw.Elapsed);
                logger.LogInformation(
                    "Problem solving completed. ProblemId={ProblemId} Solver={Solver} Outcome={Outcome} ElapsedMs={ElapsedMs}",
                    problem.Id,
                    satSolver.Name,
                    solveResult.Value.Satisfiability,
                    sw.ElapsedMilliseconds);
            }
            else if (solveResult.HasError<SolverTimeoutError>())
            {
                problem.TimeOut(sw.Elapsed);
                logger.LogWarning(
                    "Problem solving timed out. ProblemId={ProblemId} Solver={Solver} ElapsedMs={ElapsedMs}",
                    problem.Id,
                    satSolver.Name,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                problem.Fail(sw.Elapsed);
                logger.LogWarning(
                    "Problem solving failed. ProblemId={ProblemId} Solver={Solver} ErrorCount={ErrorCount} ElapsedMs={ElapsedMs}",
                    problem.Id,
                    satSolver.Name,
                    solveResult.Errors.Count,
                    sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            problem.Fail(sw.Elapsed);
            logger.LogError(
                ex,
                "Problem solving failed with exception. ProblemId={ProblemId} Solver={Solver} ElapsedMs={ElapsedMs}",
                problem.Id,
                satSolver.Name,
                sw.ElapsedMilliseconds);
        }

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}
