using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Errors;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemHandler(
    ISatProblemRepository satProblemRepository,
    IUnitOfWork unitOfWork,
    ISatSolver solver,
    IMessageBus messageBus,
    ILogger<SolveSatProblemHandler> logger
) : IRequestHandler<SolveSatProblemCommand>
{
    public async Task<Result> Handle(SolveSatProblemCommand request, CancellationToken cancellationToken)
    {
        SatProblem? satProblem = await satProblemRepository.GetById(request.SatProblemId, cancellationToken);

        if (satProblem is null)
            return Result.Fail(new NotFoundError(nameof(SatProblem), request.SatProblemId));

        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        await satProblemRepository.Update(satProblem, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        await messageBus.Publish<ISatProblemSolved>(new
        {
            SatProblemId = satProblem.Id,
            Solution = solution
        }, cancellationToken);

        return Result.Ok();
    }
}