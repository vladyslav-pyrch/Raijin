using System.Text.Json.Serialization;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColouring;

public sealed class VertexColoringSetProblemInstance(
    IValidator<VertexColoringInstanceDto>? validator = null
) : ISetProblemInstanceExtension
{
    public string ProblemType => ProblemTypes.VertexColoringProblem;

    public Result<Instance> CreateInstance(InstanceDto instanceDto)
    {
        if (instanceDto is not VertexColoringInstanceDto dto)
            throw new ArgumentException(
                $"Invalid instance DTO type. Expected {typeof(VertexColoringInstanceDto)}, got {instanceDto.GetType()}");

        var validationResult = Result.FailIfNotEmpty(
            validator?.Validate(dto).ToValidationErrors() ?? []
        );

        if (validationResult.IsFailed)
            return validationResult;

        var duplicateVertices = dto.Vertices
            .GroupBy(id => id, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        List<ValidationError> duplicateErrors = duplicateVertices
            .Select(id => new ValidationError(
                nameof(VertexColoringInstanceDto.Vertices),
                $"Duplicate vertex id: '{id}'."))
            .ToList();

        if (duplicateErrors.Count > 0)
            return Result.Fail(duplicateErrors);

        var vertices = dto.Vertices
            .Select(id => new Vertex(id))
            .ToList();

        var vertexLookup = vertices.ToDictionary(v => v.Id);

        List<IError> edgeErrors = [];
        List<Edge> edges = [];
        int edgeIndex = 0;
        foreach (VertexEdgeDto edgeDto in dto.Edges)
        {
            bool uValid = vertexLookup.TryGetValue(edgeDto.U, out Vertex? u);
            bool vValid = vertexLookup.TryGetValue(edgeDto.V, out Vertex? v);

            if (!uValid)
                edgeErrors.Add(new ValidationError(
                    $"{nameof(VertexColoringInstanceDto.Edges)}[{edgeIndex}].{nameof(VertexEdgeDto.U)}",
                    $"Vertex '{edgeDto.U}' does not exist in the vertex list."));
            if (!vValid)
                edgeErrors.Add(new ValidationError(
                    $"{nameof(VertexColoringInstanceDto.Edges)}[{edgeIndex}].{nameof(VertexEdgeDto.V)}",
                    $"Vertex '{edgeDto.V}' does not exist in the vertex list."));

            if (uValid && vValid)
                edges.Add(new Edge(u!, v!));

            edgeIndex++;
        }

        if (edgeErrors.Count > 0)
            return Result.Fail(edgeErrors);

        // Self-loop check
        List<ValidationError> selfLoopErrors = edges
            .Select((e, i) => (e, i))
            .Where(t => t.e.U == t.e.V)
            .Select(t => new ValidationError(
                $"{nameof(VertexColoringInstanceDto.Edges)}[{t.i}]",
                $"Self-loop detected: edge from vertex '{t.e.U.Id}' to itself is not allowed."))
            .ToList();

        if (selfLoopErrors.Count > 0)
            return Result.Fail(selfLoopErrors);

        // Multi-edge check (undirected: treat (u,v) and (v,u) as the same edge)
        List<ValidationError> multiEdgeErrors = edges
            .Select((e, i) => (
                pair: (string.CompareOrdinal(e.U.Id, e.V.Id) <= 0 ? e.U.Id : e.V.Id,
                       string.CompareOrdinal(e.U.Id, e.V.Id) <= 0 ? e.V.Id : e.U.Id),
                i))
            .GroupBy(t => t.pair)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1).Select(t => new ValidationError(
                $"{nameof(VertexColoringInstanceDto.Edges)}[{t.i}]",
                $"Duplicate edge between vertices '{t.pair.Item1}' and '{t.pair.Item2}' is not allowed.")))
            .ToList();

        if (multiEdgeErrors.Count > 0)
            return Result.Fail(multiEdgeErrors);

        var graph = new Graph(vertices, edges);
        return new VertexColoringInstance(graph, dto.ColorCount);
    }
}

public record VertexColoringInstanceDto(
    IReadOnlyList<string> Vertices,
    IReadOnlyList<VertexEdgeDto> Edges,
    int ColorCount
) : InstanceDto
{
    [JsonIgnore] public override string ProblemType => ProblemTypes.VertexColoringProblem;
}

public record VertexEdgeDto(string U, string V);

public sealed class VertexColoringInstanceDtoValidator : AbstractValidator<VertexColoringInstanceDto>
{
    public VertexColoringInstanceDtoValidator()
    {
        RuleFor(dto => dto.Vertices)
            .NotNull()
            .WithMessage("Vertices must not be null.");

        RuleFor(dto => dto.Edges)
            .NotNull()
            .WithMessage("Edges must not be null.");

        RuleFor(dto => dto.ColorCount)
            .GreaterThan(0)
            .WithMessage("Color count must be greater than 0.");

        RuleForEach(dto => dto.Vertices)
            .NotEmpty()
            .WithMessage("Vertex id must not be empty.");

        RuleForEach(dto => dto.Edges)
            .ChildRules(edge =>
            {
                edge.RuleFor(e => e.U)
                    .NotEmpty()
                    .WithMessage("Edge source vertex must not be empty.");

                edge.RuleFor(e => e.V)
                    .NotEmpty()
                    .WithMessage("Edge target vertex must not be empty.");
            });
    }
}
