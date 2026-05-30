using System.Text.RegularExpressions;
using FluentValidation;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Graphs;

public sealed record GraphDto(
    IReadOnlyList<VertexDto> Vertices,
    IReadOnlyList<EdgeDto> Edges
);

public sealed record VertexDto(string Id);

public sealed record EdgeDto(string Label, string U, string V);

public sealed class GraphDtoValidator : AbstractValidator<GraphDto>
{
    private static readonly Regex VariableNameRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public GraphDtoValidator()
    {
        RuleFor(dto => dto)
            .Custom((dto, context) =>
            {
                HashSet<string> vertexLookup = dto.Vertices.Select(v => v.Id).ToHashSet();

                foreach ((EdgeDto edgeDto, int index) in dto.Edges.Select((e, i) => (e, i)))
                {
                    bool uValid = vertexLookup.Contains(edgeDto.U);
                    bool vValid = vertexLookup.Contains(edgeDto.V);

                    if (!uValid)
                        context.AddFailure(
                            $"{nameof(GraphDto)}.{nameof(GraphDto.Edges)}[{index}]",
                            $"Vertex '{edgeDto.U}' does not exist in the vertex list.");
                    if (!vValid)
                        context.AddFailure(
                            $"{nameof(GraphDto)}.{nameof(GraphDto.Edges)}[{index}]",
                            $"Vertex '{edgeDto.V}' does not exist in the vertex list.");
                }

                foreach ((EdgeDto edgeDto, int index) in dto.Edges
                             .Select((e, i) => (e, i))
                             .Where(t => t.e.U == t.e.V))
                    context.AddFailure(
                        $"{nameof(GraphDto)}.{nameof(GraphDto.Edges)}[{index}]",
                        $"Self-loop detected: edge from vertex '{edgeDto.U}' to itself is not allowed.");

                foreach ((EdgeDto edgeDto, int index) in dto.Edges
                             .Select((e, i) => (e, i))
                             .GroupBy(
                                 t => string.CompareOrdinal(t.e.U, t.e.V) < 0 ? (t.e.U, t.e.V) : (t.e.V, t.e.U),
                                 tuple => tuple
                             ).Where(g => g.Count() > 1)
                             .SelectMany(g => g.Skip(1)))
                    context.AddFailure(
                        $"{nameof(GraphDto)}.{nameof(GraphDto.Edges)}[{index}]",
                        $"Duplicate edge between vertices '{edgeDto.U}' and '{edgeDto.V}' is not allowed.");
            });

        RuleFor(dto => dto.Vertices)
            .NotNull()
            .WithMessage("Vertices must not be null.")
            .Custom((vertices, context) =>
            {
                foreach (IGrouping<string, (string Id, int Index)> g in
                         vertices.Select((v, i) => (v.Id, Index: i))
                             .GroupBy(vertex => vertex.Id, StringComparer.Ordinal)
                             .Where(g => g.Count() > 1))
                    context.AddFailure(
                        $"{nameof(GraphDto)}.{nameof(GraphDto.Vertices)}[{g.Last().Index}]",
                        $"Duplicate vertex id '{g.Key}' is not allowed.");
            });

        RuleFor(dto => dto.Edges)
            .NotNull()
            .WithMessage("Edges must not be null.")
            .Custom((edges, context) =>
            {
                foreach (IGrouping<string, (string Label, int Index)> g in
                         edges.Select((e, i) => (e.Label, Index: i))
                             .GroupBy(e => e.Label, StringComparer.Ordinal)
                             .Where(g => g.Count() > 1))
                    context.AddFailure(
                        $"{nameof(GraphDto)}.{nameof(GraphDto.Edges)}[{g.Last().Index}]",
                        $"Duplicate edge label: '{g.Key}'.");
            });

        RuleForEach(dto => dto.Vertices)
            .ChildRules(vertex =>
            {
                vertex.RuleFor(v => v.Id)
                    .NotEmpty()
                    .WithMessage("Vertex id must not be empty.")
                    .Must(id => VariableNameRegex.IsMatch(id))
                    .WithMessage(
                        "Vertex id must be a valid variable name. " +
                        "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                        "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                        "Names must end with an alphanumeric character.");
            });

        RuleForEach(dto => dto.Edges)
            .ChildRules(edge =>
            {
                edge.RuleFor(e => e.Label)
                    .NotEmpty()
                    .WithMessage("Edge label must not be empty.")
                    .Must(label => VariableNameRegex.IsMatch(label))
                    .WithMessage(
                        "Edge label must be a valid variable name. " +
                        "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                        "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                        "Names must end with an alphanumeric character.");

                edge.RuleFor(e => e.U)
                    .NotEmpty()
                    .WithMessage("Edge source vertex must not be empty.");

                edge.RuleFor(e => e.V)
                    .NotEmpty()
                    .WithMessage("Edge target vertex must not be empty.");
            });
    }
}
