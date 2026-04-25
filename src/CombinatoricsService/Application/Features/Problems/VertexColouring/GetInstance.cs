using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColouring;

public sealed record GetVertexColoringInstanceQuery(Guid ProblemId)
    : IRequest<GetVertexColoringInstanceResult>;

public sealed class GetVertexColoringInstanceHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetVertexColoringInstanceQuery, GetVertexColoringInstanceResult>
{
    public async Task<Result<GetVertexColoringInstanceResult>> Handle(
        GetVertexColoringInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is not VertexColoringInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a vertex coloring instance.");

        IReadOnlyList<string> vertices = instance.Graph.Vertices.Select(v => v.Id).ToList();
        IReadOnlyList<VertexEdgeResult> edges = instance.Graph.Edges
            .Select(e => new VertexEdgeResult(e.Label, e.U.Id, e.V.Id))
            .ToList();

        return new GetVertexColoringInstanceResult(vertices, edges, instance.ColourCount);
    }
}

public sealed record GetVertexColoringInstanceResult(
    IReadOnlyList<string> Vertices,
    IReadOnlyList<VertexEdgeResult> Edges,
    int ColorCount
);

public sealed record VertexEdgeResult(string Label, string U, string V);

public sealed class GetVertexColoringInstanceValidator : AbstractValidator<GetVertexColoringInstanceQuery>
{
    public GetVertexColoringInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
