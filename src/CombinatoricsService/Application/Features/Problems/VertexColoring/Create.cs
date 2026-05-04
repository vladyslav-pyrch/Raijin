using System.Collections.Frozen;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;

public sealed class CreateVertexColoringProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateVertexColoringProblemCommand, CreateVertexColoringProblemResult>
{
    public async Task<Result<CreateVertexColoringProblemResult>> Handle(
        CreateVertexColoringProblemCommand request,
        CancellationToken cancellationToken)
    {
        List<Vertex> vertices = request.Instance.Graph.Vertices.Select(dto => new Vertex(dto.Id, dto.X, dto.Y)).ToList();
        FrozenDictionary<string, Vertex> verticesLookup = vertices.ToFrozenDictionary(dto => dto.Id);
        List<Edge> edges = request.Instance.Graph.Edges.Select(e => new Edge(
                e.Label,
                verticesLookup[e.U],
                verticesLookup[e.V]
            )
        ).ToList();

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.ProblemDetails.Name,
            request.ProblemDetails.Description,
            new VertexColoringInstance(
                new Graph(vertices, edges),
                request.Instance.ColorCount
            )
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateVertexColoringProblemResult(problem.Id);
    }
}

public sealed record CreateVertexColoringProblemCommand(
    ProblemDetailsDto ProblemDetails,
    VertexColoringInstanceDto Instance
) : IRequest<CreateVertexColoringProblemResult>;

public sealed record CreateVertexColoringProblemResult(
    Guid ProblemId
);

public sealed class CreateVertexColoringProblemValidator : AbstractValidator<CreateVertexColoringProblemCommand>
{
    public CreateVertexColoringProblemValidator(
        IValidator<VertexColoringInstanceDto> vertexColoringInstanceDtoValidator,
        IValidator<ProblemDetailsDto> problemDetailsValidator
    )
    {
        RuleFor(c => c.ProblemDetails)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(vertexColoringInstanceDtoValidator);
    }
}