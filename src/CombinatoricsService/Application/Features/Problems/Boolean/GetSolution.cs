using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public sealed record GetBooleanSolutionQuery(Guid ProblemId)
    : IRequest<GetBooleanSolutionResult>;

public sealed class GetBooleanSolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetBooleanSolutionQuery, GetBooleanSolutionResult>
{
    public async Task<Result<GetBooleanSolutionResult>> Handle(
        GetBooleanSolutionQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus is SolvingStatus.Pending or SolvingStatus.Running)
            return new DomainError("Solving is not yet complete — solution is not available.");

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new DomainError("Solving failed — no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new DomainError("Solving timed out — no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new DomainError($"Problem is in an unhandled status '{problem.SolvingStatus}'.");

        if (problem.Solution is not null && problem.Solution is not BooleanProblemSolution)
            return new DomainError(
                $"Solution type mismatch: this problem has a '{problem.Solution.GetType().Name}' solution, not a '{ProblemTypes.BooleanProblem}' solution.");

        BooleanSolutionDto? solutionDto = problem.Solution is BooleanProblemSolution boolean
            ? new BooleanSolutionDto(boolean.Assignments
                .Select(a => new BooleanVariableAssignmentDto(a.Variable.Name, a.Value))
                .ToList())
            : null;

        return new GetBooleanSolutionResult(solutionDto, problem.Satisfiability);
    }
}

public sealed record GetBooleanSolutionResult(
    BooleanSolutionDto? Solution,
    Satisfiability Satisfiability
);

public sealed record BooleanSolutionDto(IReadOnlyList<BooleanVariableAssignmentDto> Assignments);

public sealed record BooleanVariableAssignmentDto(string VariableName, bool Value);

public sealed class GetBooleanSolutionValidator : AbstractValidator<GetBooleanSolutionQuery>
{
    public GetBooleanSolutionValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
