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
            return Result.Ok();
        }

        problem.ReduceToSat();
        problem.MarkAsRunning();
        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation("Claimed problem {ProblemId} for solving.", problem.Id);

        ISatSolver? satSolver = problem.Solver is not null
            ? _satSolvers.FirstOrDefault(s => s.Name.Equals(problem.Solver, StringComparison.OrdinalIgnoreCase))
            : _satSolvers.FirstOrDefault();
        
        if (satSolver is null)
        {
            logger.LogError(
                "Problem {ProblemId} specifies solver '{Solver}' which is not registered.",
                problem.Id,
                problem.Solver);
            problem.Fail(TimeSpan.FromSeconds(0));
            await problemRepository.Update(problem, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result.Fail($"SAT solver '{problem.Solver}' is not registered.");
        }

        logger.LogInformation(
            "Using solver '{Solver}' for problem {ProblemId}.",
            satSolver.Name,
            problem.Id);
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            Result<SatSolverResult> solveResult = await satSolver.Solve(problem.SatEncoding!, cancellationToken);
            
            sw.Stop(); 
            
            if (solveResult.IsSuccess)
            {
                problem.Complete(solveResult.Value.Satisfiability, solveResult.Value.Assignment, sw.Elapsed);
                logger.LogInformation("Problem {ProblemId} completed successfully.", problem.Id);
            }
            else if (solveResult.HasError<SolverTimeoutError>())
            {
                problem.TimeOut(sw.Elapsed);
                logger.LogWarning("Problem {ProblemId} timed out.", problem.Id);
            }
            else
            {
                problem.Fail(sw.Elapsed);
                logger.LogWarning("Problem {ProblemId} failed: {Errors}", problem.Id, solveResult.Errors);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            problem.Fail(sw.Elapsed);
            logger.LogError(ex, "Problem {ProblemId} failed with exception.", problem.Id);
        }

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}