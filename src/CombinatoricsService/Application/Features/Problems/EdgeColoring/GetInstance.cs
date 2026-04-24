using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;

public sealed record GetEdgeColoringInstanceQuery(Guid ProblemId)
    : IRequest<GetEdgeColoringInstanceResult>;

public sealed class GetEdgeColoringInstanceHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetEdgeColoringInstanceQuery, GetEdgeColoringInstanceResult>
{
    public async Task<Result<GetEdgeColoringInstanceResult>> Handle(
        GetEdgeColoringInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is not EdgeColoringInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have an edge coloring instance.");

        IReadOnlyList<string> vertices = instance.Graph.Vertices.Select(v => v.Id).ToList();
        IReadOnlyList<EdgeResult> edges = instance.Graph.Edges
            .Select(e => new EdgeResult(e.Label, e.U.Id, e.V.Id))
            .ToList();

        return new GetEdgeColoringInstanceResult(vertices, edges, instance.ColourCount);
    }
}

public sealed record GetEdgeColoringInstanceResult(
    IReadOnlyList<string> Vertices,
    IReadOnlyList<EdgeResult> Edges,
    int ColorCount
);

public sealed record EdgeResult(string Label, string U, string V);

public sealed class GetEdgeColoringInstanceValidator : AbstractValidator<GetEdgeColoringInstanceQuery>
{
    public GetEdgeColoringInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
