using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Graphs;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;

public sealed record GetVertexColoringInstanceQuery(Guid ProblemId)
    : IRequest<GetVertexColoringInstanceResult>;

public sealed class GetVertexColoringInstanceHandler(
    IProblemRepository problemRepository,
    ILogger<GetVertexColoringInstanceHandler> logger
) : IRequestHandler<GetVertexColoringInstanceQuery, GetVertexColoringInstanceResult>
{
    public async Task<Result<GetVertexColoringInstanceResult>> Handle(
        GetVertexColoringInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (problem.Instance is not VertexColoringInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a vertex coloring instance.");

        IReadOnlyList<VertexDto> vertices = instance.Graph.Vertices
            .Select(v => new VertexDto(v.Id))
            .ToList();
        IReadOnlyList<EdgeDto> edges = instance.Graph.Edges
            .Select(e => new EdgeDto(e.Label, e.U.Id, e.V.Id))
            .ToList();

        var graph = new GraphDto(vertices, edges);
        logger.LogDebug(
            "Vertex coloring instance read. ProblemId={ProblemId} VertexCount={VertexCount} EdgeCount={EdgeCount} ColorCount={ColorCount}",
            request.ProblemId,
            vertices.Count,
            edges.Count,
            instance.ColourCount);

        return new GetVertexColoringInstanceResult(new VertexColoringInstanceDto(graph, instance.ColourCount));
    }
}

public sealed record GetVertexColoringInstanceResult(VertexColoringInstanceDto Instance);

public sealed class GetVertexColoringInstanceValidator : AbstractValidator<GetVertexColoringInstanceQuery>
{
    public GetVertexColoringInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
