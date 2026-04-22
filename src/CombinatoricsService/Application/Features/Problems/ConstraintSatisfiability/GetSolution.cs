using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed record GetCspSolutionQuery(Guid ProblemId)
    : IRequest<GetCspSolutionResult>;

public sealed class GetCspSolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetCspSolutionQuery, GetCspSolutionResult>
{
    public async Task<Result<GetCspSolutionResult>> Handle(
        GetCspSolutionQuery request,
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

        if (problem.Solution is not null && problem.Solution is not CspSolution)
            return new DomainError(
                $"Solution type mismatch: this problem has a '{problem.Solution.GetType().Name}' solution, not a '{ProblemTypes.ConstraintSatisfiabilityProblem}' solution.");

        CspSolutionDto? solutionDto = problem.Solution is CspSolution csp
            ? new CspSolutionDto(
                csp.Configuration
                    .Select(a => new DecisionVariableStateAssignmentDto(a.Name, a.AssignedState))
                    .ToList(),
                csp.AuxiliaryAssignments
                    .Select(a => new CspBooleanVariableAssignmentDto(a.Variable.Name, a.Value))
                    .ToList())
            : null;

        return new GetCspSolutionResult(solutionDto, problem.Satisfiability);
    }
}

public sealed record GetCspSolutionResult(
    CspSolutionDto? Solution,
    Satisfiability Satisfiability
);

public sealed record CspSolutionDto(
    IReadOnlyList<DecisionVariableStateAssignmentDto> Configuration,
    IReadOnlyList<CspBooleanVariableAssignmentDto> AuxiliaryAssignments
);

public sealed record DecisionVariableStateAssignmentDto(string Name, string AssignedState);

public sealed record CspBooleanVariableAssignmentDto(string VariableName, bool Value);

public sealed class GetCspSolutionValidator : AbstractValidator<GetCspSolutionQuery>
{
    public GetCspSolutionValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
