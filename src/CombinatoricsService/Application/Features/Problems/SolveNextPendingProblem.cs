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
    ISatSolver satSolver,
    IUnitOfWork unitOfWork,
    ILogger<SolveNextPendingProblemHandler> logger
) : IRequestHandler<SolveNextPendingProblemCommand>
{
    public async Task<Result> Handle(SolveNextPendingProblemCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransaction(cancellationToken);

        Problem? problem = await problemRepository.GetOldestPendingWithLock(cancellationToken);

        if (problem is null)
        {
            await unitOfWork.Commit(cancellationToken);
            return Result.Ok();
        }

        problem.MarkAsRunning();
        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation("Claimed problem {ProblemId} for solving.", problem.Id);

        if (problem.SatEncoding is null)
        {
            logger.LogError(
                "Problem {ProblemId} is Pending but has no SAT encoding — marking as Failed.",
                problem.Id);
            problem.Fail();
            await problemRepository.Update(problem, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result.Fail($"Problem {problem.Id} has no SAT encoding and cannot be solved.");
        }

        try
        {
            Result<SolveResult> solveResult = await satSolver.Solve(problem.SatEncoding, cancellationToken);

            if (solveResult.IsSuccess)
            {
                problem.Complete(solveResult.Value.Satisfiability, solveResult.Value.Assignment);
                logger.LogInformation("Problem {ProblemId} completed successfully.", problem.Id);
            }
            else
            {
                problem.Fail();
                logger.LogWarning("Problem {ProblemId} failed: {Errors}", problem.Id, solveResult.Errors);
            }
        }
        catch (Exception ex)
        {
            problem.Fail();
            logger.LogError(ex, "Problem {ProblemId} failed with exception.", problem.Id);
        }

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}
