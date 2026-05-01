using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;

public sealed record GetVertexColoringSolutionQuery(Guid ProblemId)
    : IRequest<GetVertexColoringSolutionResult>;

public sealed class GetVertexColoringSolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetVertexColoringSolutionQuery, GetVertexColoringSolutionResult>
{
    public async Task<Result<GetVertexColoringSolutionResult>> Handle(
        GetVertexColoringSolutionQuery request,
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

        if (problem.Solution is not null && problem.Solution is not VertexColoringSolution)
            return new NotFoundError(
                $"Solution type mismatch: this problem has a {problem.Instance.ProblemType()} solution, not a '{ProblemTypes.VertexColoringProblem}' solution.");

        VertexColoringSolutionDto? solutionDto = problem.Solution is VertexColoringSolution vertexColoring
            ? new VertexColoringSolutionDto(vertexColoring.ColorAssignments
                .Select(a => new VertexColorAssignmentDto(a.VertexId, a.Color))
                .ToList())
            : null;

        return new GetVertexColoringSolutionResult(solutionDto, problem.Satisfiability);
    }
}

public sealed record GetVertexColoringSolutionResult(
    VertexColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);

public sealed record VertexColoringSolutionDto(IReadOnlyList<VertexColorAssignmentDto> ColorAssignments);

public sealed record VertexColorAssignmentDto(string VertexId, int Color);

public sealed class GetVertexColoringSolutionValidator : AbstractValidator<GetVertexColoringSolutionQuery>
{
    public GetVertexColoringSolutionValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}