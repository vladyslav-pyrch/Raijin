using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing.DimacsToGraph;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;

public sealed class CreateVertexColoringFromDimacsHandler(
    IDimacsToGraphParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateVertexColoringFromDimacsCommand, CreateVertexColoringFromDimacsResult>
{
    public async Task<Result<CreateVertexColoringFromDimacsResult>> Handle(
        CreateVertexColoringFromDimacsCommand request,
        CancellationToken cancellationToken)
    {
        Result<Graph> parseResult = parser.Parse(request.Dimacs);

        if (parseResult.IsFailed)
            return Result.Fail(parseResult.Errors
                .Select(error => new ValidationError(nameof(request.Dimacs), error.Message)));

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.ProblemDetails.Name,
            request.ProblemDetails.Description,
            new VertexColoringInstance(parseResult.Value, request.ColorCount)
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateVertexColoringFromDimacsResult(problem.Id);
    }
}

public sealed record CreateVertexColoringFromDimacsCommand(
    ProblemDetailsDto ProblemDetails,
    string Dimacs,
    int ColorCount
) : IRequest<CreateVertexColoringFromDimacsResult>;

public sealed record CreateVertexColoringFromDimacsResult(Guid ProblemId);

public sealed class CreateVertexColoringFromDimacsValidator : AbstractValidator<CreateVertexColoringFromDimacsCommand>
{
    public CreateVertexColoringFromDimacsValidator(IValidator<ProblemDetailsDto> problemDetailsValidator)
    {
        RuleFor(c => c.ProblemDetails)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Dimacs)
            .NotEmpty();
        RuleFor(c => c.ColorCount)
            .GreaterThan(0);
    }
}
