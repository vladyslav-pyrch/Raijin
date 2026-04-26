using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;

public sealed class CreateEdgeColoringProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateEdgeColoringProblemCommand, CreateEdgeColoringProblemResult>
{
    public async Task<Result<CreateEdgeColoringProblemResult>> Handle(
        CreateEdgeColoringProblemCommand request,
        CancellationToken cancellationToken)
    {
        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.ProblemDetails.Name,
            request.ProblemDetails.Description,
            new EdgeColoringInstance(
                new Graph(
                    request.Instance.Graph.Vertices.Select(dto => new Vertex(dto.Id)).ToList(),
                    request.Instance.Graph.Edges.Select(e => new Edge(
                            e.Label,
                            new Vertex(e.U),
                            new Vertex(e.V)
                        )
                    ).ToList()
                ),
                request.Instance.ColorCount
            )
        );
        
        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateEdgeColoringProblemResult(problem.Id);
    }
}

public sealed record CreateEdgeColoringProblemCommand(
    ProblemDetailsDto ProblemDetails,
    EdgeColoringInstanceDto Instance
) : IRequest<CreateEdgeColoringProblemResult>;

public sealed record CreateEdgeColoringProblemResult(Guid ProblemId);

public sealed class CreateEdgeColoringProblemValidator : AbstractValidator<CreateEdgeColoringProblemCommand>
{
    public CreateEdgeColoringProblemValidator(
        IValidator<EdgeColoringInstanceDto> edgeColoringInstanceDtoValidator,
        IValidator<ProblemDetailsDto> problemDetailsValidator
        )
    {
        RuleFor(c => c.ProblemDetails)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(edgeColoringInstanceDtoValidator);
    }
}
