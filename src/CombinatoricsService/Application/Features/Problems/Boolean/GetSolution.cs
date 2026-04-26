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

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new NotFoundError("Solving failed — no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new NotFoundError("Solving timed out — no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new NotFoundError("Solving is not complete — no solution is available.");

        if (problem.Solution is not null && problem.Solution is not BooleanProblemSolution)
            return new NotFoundError(
                $"Solution type mismatch: this problem has a {problem.Instance.ProblemType()} solution, not a '{ProblemTypes.BooleanProblem}' solution.");

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
