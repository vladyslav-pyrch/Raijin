using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class SolveProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SolveProblemCommand, SolveProblemResult>
{
    public async Task<Result<SolveProblemResult>> Handle(
        SolveProblemCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus is SolvingStatus.Running)
            return new ConflictError("Cannot reduce to SAT while solving is in progress.");

        problem.Solve(request.Solver);

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new SolveProblemResult(problem.Id);
    }
}

public sealed record SolveProblemCommand(
    Guid ProblemId,
    string Solver
) : IRequest<SolveProblemResult>;

public sealed record SolveProblemResult(
    Guid ProblemId
);

public sealed class SolveProblemValidator : AbstractValidator<SolveProblemCommand>
{
    public SolveProblemValidator()
    {
        RuleFor(command => command.ProblemId)
            .NotEmpty();
        RuleFor(command => command.Solver)
            .NotEmpty()
            .MaximumLength(100);
    }
}
