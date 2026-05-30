using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed record GetCspSolutionQuery(Guid ProblemId)
    : IRequest<GetCspSolutionResult>;

public sealed class GetCspSolutionHandler(
    IProblemRepository problemRepository,
    ILogger<GetCspSolutionHandler> logger
) : IRequestHandler<GetCspSolutionQuery, GetCspSolutionResult>
{
    public async Task<Result<GetCspSolutionResult>> Handle(
        GetCspSolutionQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new NotFoundError("Solving failed — no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new NotFoundError("Solving timed out — no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new NotFoundError("Solving is not complete — no solution is available.");

        if (problem.Solution is not null && problem.Solution is not CspSolution)
            return new NotFoundError(
                $"Solution type mismatch: this problem has a {problem.Instance.ProblemType()} solution, not a '{ProblemTypes.ConstraintSatisfiabilityProblem}' solution.");

        CspSolutionDto? solutionDto = problem.Solution is CspSolution csp
            ? new CspSolutionDto(
                csp.Configuration
                    .Select(a => new DecisionVariableStateAssignmentDto(a.Name, a.AssignedState))
                    .ToList(),
                csp.AuxiliaryAssignments
                    .Select(a => new CspBooleanVariableAssignmentDto(a.Variable.Name, a.Value))
                    .ToList())
            : null;

        logger.LogDebug(
            "CSP solution read. ProblemId={ProblemId} Satisfiability={Satisfiability} HasSolution={HasSolution} ConfigurationCount={ConfigurationCount} AuxiliaryAssignmentCount={AuxiliaryAssignmentCount}",
            request.ProblemId,
            problem.Satisfiability,
            solutionDto is not null,
            solutionDto?.Configuration.Count ?? 0,
            solutionDto?.AuxiliaryAssignments.Count ?? 0);

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
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
