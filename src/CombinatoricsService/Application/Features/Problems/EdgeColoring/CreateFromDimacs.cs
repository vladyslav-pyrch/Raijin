using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing.DimacsToGraph;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

namespace Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;

public sealed class CreateEdgeColoringFromDimacsHandler(
    IDimacsToGraphParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateEdgeColoringFromDimacsHandler> logger
) : IRequestHandler<CreateEdgeColoringFromDimacsCommand, CreateEdgeColoringFromDimacsResult>
{
    public async Task<Result<CreateEdgeColoringFromDimacsResult>> Handle(
        CreateEdgeColoringFromDimacsCommand request,
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
            new EdgeColoringInstance(parseResult.Value, request.ColorCount)
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Problem created from DIMACS. ProblemId={ProblemId} ProblemType={ProblemType} ColorCount={ColorCount}",
            problem.Id,
            "edge-coloring",
            request.ColorCount);

        return new CreateEdgeColoringFromDimacsResult(problem.Id);
    }
}

public sealed record CreateEdgeColoringFromDimacsCommand(
    ProblemDetailsDto ProblemDetails,
    string Dimacs,
    int ColorCount
) : IRequest<CreateEdgeColoringFromDimacsResult>;

public sealed record CreateEdgeColoringFromDimacsResult(Guid ProblemId);

public sealed class CreateEdgeColoringFromDimacsValidator : AbstractValidator<CreateEdgeColoringFromDimacsCommand>
{
    public CreateEdgeColoringFromDimacsValidator(IValidator<ProblemDetailsDto> problemDetailsValidator)
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
