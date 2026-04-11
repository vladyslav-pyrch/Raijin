using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Domain.SatRuns;

namespace Raijin.CombinatoricsService.Application.Features.SatRuns;

public sealed record SolveNextPendingSatRunCommand : IRequest;

public sealed class SolveNextPendingSatRunHandler(
    ISatRunRepository satRunRepository,
    ISatSolver satSolver,
    IUnitOfWork unitOfWork,
    ILogger<SolveNextPendingSatRunHandler> logger
) : IRequestHandler<SolveNextPendingSatRunCommand>
{
    public async Task<Result> Handle(SolveNextPendingSatRunCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransaction(cancellationToken);

        SatRun? satRun = await satRunRepository.GetOldestPendingWithLock(cancellationToken);

        if (satRun is null)
        {
            await unitOfWork.Commit(cancellationToken);
            return Result.Ok();
        }

        satRun.MarkAsRunning();
        await satRunRepository.Update(satRun, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation("Claimed SAT run {SatRunId} for solving.", satRun.Id);

        try
        {
            Result<IReadOnlyList<int>> solveResult = await satSolver.Solve(satRun.SatEncoding, cancellationToken);

            if (solveResult.IsSuccess)
            {
                satRun.Complete(Satisfiability.Satisfiable, solveResult.Value);
                logger.LogInformation("SAT run {SatRunId} completed successfully.", satRun.Id);
            }
            else
            {
                satRun.Fail();
                logger.LogWarning("SAT run {SatRunId} failed: {Errors}", satRun.Id, solveResult.Errors);
            }
        }
        catch (Exception ex)
        {
            satRun.Fail();
            logger.LogError(ex, "SAT run {SatRunId} failed with exception.", satRun.Id);
        }

        await satRunRepository.Update(satRun, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}
