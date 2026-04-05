using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SatProblems;

public sealed class SolveNextPendingSatProblemHandler(
    ISatSolver solver,
    ISatProblemJobRepository satProblemJobRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<SolveNextPendingSatProblemHandler> logger
) : IRequestHandler<SolveNextPendingSatProblemCommand>
{
    public async Task<Result> Handle(SolveNextPendingSatProblemCommand request, CancellationToken cancellationToken)
    {
        SatProblem? satProblem = await satProblemJobRepository.GetNextPendingAndLock(cancellationToken);

        if (satProblem is null)
            return Result.Ok();

        satProblem.MarkAsSolving();

        await satProblemJobRepository.Update(satProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        try
        {
            int[] solution = await solver.Solve(satProblem, cancellationToken);
            satProblem.Solve(solution);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to solve sat problem with id: {Id}", satProblem.Id);
            satProblem.Fail();
        }

        await satProblemJobRepository.Update(satProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<ISatProblemSolved>(new
        {
            ProblemId = satProblem.Id,
            Solution = satProblem.Solution.Assignments,
            satProblem.Solution.Satisfiability
        }, cancellationToken);

        return Result.Ok();
    }
}

public sealed record SolveNextPendingSatProblemCommand : IRequest;