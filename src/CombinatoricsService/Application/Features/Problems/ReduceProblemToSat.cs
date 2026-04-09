using FluentResults;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class ReduceProblemToSatCommandHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReduceProblemToSatCommand>
{
    public async Task<Result> Handle(ReduceProblemToSatCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is null)
            return new ConflictError($"Problem with id {request.ProblemId} does not have an instance set");

        if (problem.SatEncoding is not null)
            return new ConflictError($"Problem with id {request.ProblemId} already has a sat reduction");

        problem.ReduceToSat();

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record ReduceProblemToSatCommand(
    Guid ProblemId
) : IRequest;