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

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new NotFoundError("Solving failed — no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new NotFoundError("Solving timed out — no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new NotFoundError("Solving is not complete — no solution is available.");

        if (problem.Solution is not null && problem.Solution is not BooleanSatisfiabilitySolution)
            return new NotFoundError(
                $"Solution type mismatch: this problem has a {problem.Instance.ProblemType()} solution, not a '{ProblemTypes.BooleanSatisfiabilityProblem}' solution.");

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
