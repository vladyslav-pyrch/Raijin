using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;

public sealed record GetEdgeColoringSolutionQuery(Guid ProblemId)
    : IRequest<GetEdgeColoringSolutionResult>;

public sealed class GetEdgeColoringSolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetEdgeColoringSolutionQuery, GetEdgeColoringSolutionResult>
{
    public async Task<Result<GetEdgeColoringSolutionResult>> Handle(
        GetEdgeColoringSolutionQuery request,
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

        if (problem.Solution is not null && problem.Solution is not EdgeColoringSolution)
            return new DomainError(
                $"Solution type mismatch: this problem has a '{problem.Solution.GetType().Name}' solution, not a '{ProblemTypes.EdgeColoringProblem}' solution.");

        EdgeColoringSolutionDto? solutionDto = problem.Solution is EdgeColoringSolution edgeColoring
            ? new EdgeColoringSolutionDto(edgeColoring.ColorAssignments
                .Select(a => new EdgeColorAssignmentDto(a.EdgeLabel, a.Color))
                .ToList())
            : null;

        return new GetEdgeColoringSolutionResult(solutionDto, problem.Satisfiability);
    }
}

public sealed record GetEdgeColoringSolutionResult(
    EdgeColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);

public sealed record EdgeColoringSolutionDto(IReadOnlyList<EdgeColorAssignmentDto> ColorAssignments);

public sealed record EdgeColorAssignmentDto(string EdgeLabel, int Color);

public sealed class GetEdgeColoringSolutionValidator : AbstractValidator<GetEdgeColoringSolutionQuery>
{
    public GetEdgeColoringSolutionValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
