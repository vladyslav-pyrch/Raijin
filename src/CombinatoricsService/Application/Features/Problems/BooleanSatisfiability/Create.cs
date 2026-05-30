using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public sealed class CreateSatProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateSatProblemHandler> logger
) : IRequestHandler<CreateSatProblemCommand, CreateSatProblemResult>
{
    public async Task<Result<CreateSatProblemResult>> Handle(
        CreateSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        IEnumerable<List<Literal>> literals = request.Instance.Clauses.Select(clause => clause
            .Select(literalStr =>
            {
                bool negated = literalStr.StartsWith('~');
                string name = negated ? literalStr[1..] : literalStr;
                return new Literal(new SatVariable(name), negated);
            }).ToList());

        List<Clause> clauses = literals.Select(clauseLiterals => new Clause(clauseLiterals)).ToList();

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.Details.Name,
            request.Details.Description,
            new BooleanSatisfiabilityInstance(clauses)
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Problem created. ProblemId={ProblemId} ProblemType={ProblemType} ClauseCount={ClauseCount}",
            problem.Id,
            "sat",
            clauses.Count);

        return new CreateSatProblemResult(problem.Id);
    }
}

public sealed record CreateSatProblemCommand(
    ProblemDetailsDto Details,
    SatInstanceDto Instance
) : IRequest<CreateSatProblemResult>;

public sealed record CreateSatProblemResult(
    Guid ProblemId
);

public sealed class CreateSatProblemValidator
    : AbstractValidator<CreateSatProblemCommand>
{
    public CreateSatProblemValidator(
        IValidator<ProblemDetailsDto> problemDetailsValidator,
        IValidator<SatInstanceDto> booleanSatisfiabilityInstanceValidator)
    {
        RuleFor(dto => dto.Details)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(booleanSatisfiabilityInstanceValidator);
    }
}
