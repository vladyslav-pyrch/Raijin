using FluentResults;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class SolveProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<SolveProblemCommand>
{
    public async Task<Result> Handle(SolveProblemCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is null)
            return new ConflictError("Cannot solve problem without an instance");

        if (problem.SatEncoding is null)
            return new ConflictError("Cannot solve problem without sat encoding");

        if (problem.SatRun is not null)
            return new ConflictError("Cannot solve problem more than one time for the same instance");

        problem.StartSatRun();
        problem.MarkSatRunAsRunning();

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Send<ISubmitSatProblem>(new
        {
            ProblemId = problem.Id, problem.SatEncoding.Clauses
        }, cancellationToken);

        return Result.Ok();
    }
}

public sealed record SolveProblemCommand(Guid ProblemId) : IRequest;