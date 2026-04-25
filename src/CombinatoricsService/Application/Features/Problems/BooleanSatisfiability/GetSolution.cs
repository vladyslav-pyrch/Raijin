using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public sealed record GetBooleanSatisfiabilitySolutionQuery(Guid ProblemId)
    : IRequest<GetBooleanSatisfiabilitySolutionResult>;

public sealed class GetBooleanSatisfiabilitySolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetBooleanSatisfiabilitySolutionQuery, GetBooleanSatisfiabilitySolutionResult>
{
    public async Task<Result<GetBooleanSatisfiabilitySolutionResult>> Handle(
        GetBooleanSatisfiabilitySolutionQuery request,
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

        if (problem.Solution is not null && problem.Solution is not BooleanSatisfiabilitySolution)
            return new DomainError(
                $"Solution type mismatch: this problem has a '{problem.Solution.GetType().Name}' solution, not a '{ProblemTypes.BooleanSatisfiabilityProblem}' solution.");

        BooleanSatisfiabilitySolutionDto? solutionDto = problem.Solution is BooleanSatisfiabilitySolution sat
            ? new BooleanSatisfiabilitySolutionDto(sat.Assignments
                .Select(a => new SatVariableAssignmentDto(a.Variable.Name, a.Value))
                .ToList())
            : null;

        return new GetBooleanSatisfiabilitySolutionResult(solutionDto, problem.Satisfiability);
    }
}

public sealed record GetBooleanSatisfiabilitySolutionResult(
    BooleanSatisfiabilitySolutionDto? Solution,
    Satisfiability Satisfiability
);

public sealed record BooleanSatisfiabilitySolutionDto(IReadOnlyList<SatVariableAssignmentDto> Assignments);

public sealed record SatVariableAssignmentDto(string VariableName, bool Value);

public sealed class GetBooleanSatisfiabilitySolutionValidator
    : AbstractValidator<GetBooleanSatisfiabilitySolutionQuery>
{
    public GetBooleanSatisfiabilitySolutionValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
