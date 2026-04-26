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

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new NotFoundError("Solving failed — no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new NotFoundError("Solving timed out — no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new NotFoundError("Solving is not complete — no solution is available.");

        if (problem.Solution is not null && problem.Solution is not EdgeColoringSolution)
            return new NotFoundError(
                $"Solution type mismatch: this problem has a {problem.Instance.ProblemType()} solution, not a '{ProblemTypes.EdgeColoringProblem}' solution.");

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
