using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Graphs;
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
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (problem.Instance is not EdgeColoringInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have an edge coloring instance.");

        IReadOnlyList<VertexDto> vertices = instance.Graph.Vertices
            .Select(v => new VertexDto(v.Id))
            .ToList();
        IReadOnlyList<EdgeDto> edges = instance.Graph.Edges
            .Select(e => new EdgeDto(e.Label, e.U.Id, e.V.Id))
            .ToList();

        var graph = new GraphDto(vertices, edges);

        return new GetEdgeColoringInstanceResult(
            new EdgeColoringInstanceDto(graph, instance.ColourCount)
        );
    }
}

public sealed record GetEdgeColoringInstanceResult(EdgeColoringInstanceDto Instance);

public sealed class GetEdgeColoringInstanceValidator : AbstractValidator<GetEdgeColoringInstanceQuery>
{
    public GetEdgeColoringInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
