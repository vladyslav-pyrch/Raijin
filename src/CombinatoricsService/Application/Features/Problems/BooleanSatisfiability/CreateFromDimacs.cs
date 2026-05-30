using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing.DimacsToSat;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public sealed class CreateBooleanSatisfiabilityFromDimacsHandler(
    IDimacsToSatParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateBooleanSatisfiabilityFromDimacsHandler> logger
) : IRequestHandler<CreateBooleanSatisfiabilityFromDimacsCommand, CreateBooleanSatisfiabilityFromDimacsResult>
{
    public async Task<Result<CreateBooleanSatisfiabilityFromDimacsResult>> Handle(
        CreateBooleanSatisfiabilityFromDimacsCommand request,
        CancellationToken cancellationToken)
    {
        Result<BooleanSatisfiabilityInstance> parseResult = parser.Parse(request.Dimacs);

        if (parseResult.IsFailed)
            return Result.Fail(parseResult.Errors
                .Select(error => new ValidationError(nameof(request.Dimacs), error.Message)));

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.ProblemDetails.Name,
            request.ProblemDetails.Description,
            parseResult.Value
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Problem created from DIMACS. ProblemId={ProblemId} ProblemType={ProblemType}",
            problem.Id,
            "sat");

        return new CreateBooleanSatisfiabilityFromDimacsResult(problem.Id);
    }
}

public sealed record CreateBooleanSatisfiabilityFromDimacsCommand(
    ProblemDetailsDto ProblemDetails,
    string Dimacs
) : IRequest<CreateBooleanSatisfiabilityFromDimacsResult>;

public sealed record CreateBooleanSatisfiabilityFromDimacsResult(Guid ProblemId);

public sealed class CreateBooleanSatisfiabilityFromDimacsValidator : AbstractValidator<CreateBooleanSatisfiabilityFromDimacsCommand>
{
    public CreateBooleanSatisfiabilityFromDimacsValidator(IValidator<ProblemDetailsDto> problemDetailsValidator)
    {
        RuleFor(c => c.ProblemDetails)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Dimacs)
            .NotEmpty();
    }
}
