using System.Text.RegularExpressions;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Patterns;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColouring;

public sealed record SetVertexColoringProblemInstanceCommand(
    Guid ProblemId,
    VertexColoringInstanceDto Instance
) : IRequest;

public sealed class SetVertexColoringProblemInstanceHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetVertexColoringProblemInstanceCommand>
{
    public async Task<Result> Handle(
        SetVertexColoringProblemInstanceCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus == SolvingStatus.Running)
            return new ConflictError("Cannot change instance while solving is in progress.");

        List<ValidationError> duplicateVertexErrors = request.Instance.Vertices
            .GroupBy(id => id, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => new ValidationError(
                $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Vertices)}",
                $"Duplicate vertex id: '{g.Key}'."))
            .ToList();

        if (duplicateVertexErrors.Count > 0)
            return Result.Fail(duplicateVertexErrors);

        List<ValidationError> duplicateEdgeLabelErrors = request.Instance.Edges
            .GroupBy(e => e.Label, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => new ValidationError(
                $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Edges)}",
                $"Duplicate edge label: '{g.Key}'."))
            .ToList();

        if (duplicateEdgeLabelErrors.Count > 0)
            return Result.Fail(duplicateEdgeLabelErrors);

        List<Vertex> vertices = request.Instance.Vertices
            .Select(id => new Vertex(id))
            .ToList();

        Dictionary<string, Vertex> vertexLookup = vertices.ToDictionary(v => v.Id);

        List<IError> edgeErrors = [];
        List<Edge> edges = [];
        int edgeIndex = 0;

        foreach (VertexColoringEdgeDto edgeDto in request.Instance.Edges)
        {
            bool uValid = vertexLookup.TryGetValue(edgeDto.U, out Vertex? u);
            bool vValid = vertexLookup.TryGetValue(edgeDto.V, out Vertex? v);

            if (!uValid)
                edgeErrors.Add(new ValidationError(
                    $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Edges)}[{edgeIndex}].{nameof(VertexColoringEdgeDto.U)}",
                    $"Vertex '{edgeDto.U}' does not exist in the vertex list."));
            if (!vValid)
                edgeErrors.Add(new ValidationError(
                    $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Edges)}[{edgeIndex}].{nameof(VertexColoringEdgeDto.V)}",
                    $"Vertex '{edgeDto.V}' does not exist in the vertex list."));

            if (uValid && vValid)
                edges.Add(new Edge(edgeDto.Label, u!, v!));

            edgeIndex++;
        }

        if (edgeErrors.Count > 0)
            return Result.Fail(edgeErrors);

        List<ValidationError> selfLoopErrors = edges
            .Select((e, i) => (e, i))
            .Where(t => t.e.U == t.e.V)
            .Select(t => new ValidationError(
                $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Edges)}[{t.i}]",
                $"Self-loop detected: edge from vertex '{t.e.U.Id}' to itself is not allowed."))
            .ToList();

        if (selfLoopErrors.Count > 0)
            return Result.Fail(selfLoopErrors);

        List<ValidationError> multiEdgeErrors = edges
            .Select((e, i) => (
                pair: (string.CompareOrdinal(e.U.Id, e.V.Id) <= 0 ? e.U.Id : e.V.Id,
                       string.CompareOrdinal(e.U.Id, e.V.Id) <= 0 ? e.V.Id : e.U.Id),
                i))
            .GroupBy(t => t.pair)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1).Select(t => new ValidationError(
                $"{nameof(request.Instance)}.{nameof(VertexColoringInstanceDto.Edges)}[{t.i}]",
                $"Duplicate edge between vertices '{t.pair.Item1}' and '{t.pair.Item2}' is not allowed.")))
            .ToList();

        if (multiEdgeErrors.Count > 0)
            return Result.Fail(multiEdgeErrors);

        problem.SetInstance(new VertexColoringInstance(new Graph(vertices, edges), request.Instance.ColorCount));

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record VertexColoringInstanceDto(
    IReadOnlyList<string> Vertices,
    IReadOnlyList<VertexColoringEdgeDto> Edges,
    int ColorCount
);

public sealed record VertexColoringEdgeDto(string Label, string U, string V);

public sealed class VertexColoringInstanceDtoValidator : AbstractValidator<VertexColoringInstanceDto>
{
    private static readonly Regex VariableNameRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

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
            .WithMessage("Vertex id must not be empty.")
            .Must(id => id != null && VariableNameRegex.IsMatch(id))
            .WithMessage(
                "Vertex id must be a valid variable name. " +
                "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                "Names must end with an alphanumeric character.");

        RuleForEach(dto => dto.Edges)
            .ChildRules(edge =>
            {
                edge.RuleFor(e => e.Label)
                    .NotEmpty()
                    .WithMessage("Edge label must not be empty.")
                    .Must(label => label != null && VariableNameRegex.IsMatch(label))
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

public sealed class SetVertexColoringProblemInstanceValidator : AbstractValidator<SetVertexColoringProblemInstanceCommand>
{
    public SetVertexColoringProblemInstanceValidator()
    {
        RuleFor(c => c.ProblemId).NotEmpty();
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(new VertexColoringInstanceDtoValidator());
    }
}
