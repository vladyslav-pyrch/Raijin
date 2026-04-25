using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class ReduceToSatHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReduceToSatCommand, ReduceToSatResult>
{
    public async Task<Result<ReduceToSatResult>> Handle(
        ReduceToSatCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus is SolvingStatus.Running)
            return new ConflictError("Cannot reduce to SAT while solving is in progress.");

        if (problem.Instance is null)
            return new DomainError("Problem has no instance set. Set an instance via PUT /problems/{id}/instance first.");

        problem.SetSolver(request.Solver);
        problem.ReduceToSat();

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new ReduceToSatResult(problem.Id);
    }
}

public sealed record ReduceToSatCommand(
    Guid ProblemId,
    string Solver
) : IRequest<ReduceToSatResult>;

public sealed record ReduceToSatResult(
    Guid ProblemId
);

public sealed class ReduceToSatValidator : AbstractValidator<ReduceToSatCommand>
{
    public ReduceToSatValidator()
    {
        RuleFor(command => command.ProblemId)
            .NotEmpty();
        RuleFor(command => command.Solver)
            .NotEmpty()
            .MaximumLength(100);
    }
}
