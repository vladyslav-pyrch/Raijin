using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed class CreateCspProblemHandler(
    IStringToBoolExprParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCspProblemCommand, CreateCspProblemResult>
{
    public async Task<Result<CreateCspProblemResult>> Handle(
        CreateCspProblemCommand request,
        CancellationToken cancellationToken)
    {
        List<string> variableNames = request.Instance.Variables.Select(v => v.Name).ToList();

        List<ValidationError> duplicateErrors = variableNames
            .GroupBy(name => name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => new ValidationError(
                $"{nameof(request.Instance)}.{nameof(CspInstanceDto.Variables)}",
                $"Duplicate variable name: '{g.Key}'."))
            .ToList();

        if (duplicateErrors.Count > 0)
            return Result.Fail(duplicateErrors);

        List<DecisionVariable> variables = request.Instance.Variables
            .Select(v => new DecisionVariable(v.Name, v.States.ToList()))
            .ToList();

        List<BoolExpr> constraints = [];
        List<IError> parseErrors = [];

        var constraintIndex = 0;
        foreach (string constraintFormula in request.Instance.Constraints)
        {
            Result<BoolExpr> parseResult = parser.Parse(constraintFormula);
            if (parseResult.IsFailed)
                parseErrors.AddRange(parseResult.Errors.Select(e =>
                    new ValidationError(
                        $"{nameof(request.Instance)}.{nameof(CspInstanceDto.Constraints)}[{constraintIndex}]",
                        e.Message)));
            else
                constraints.Add(parseResult.Value);

            constraintIndex++;
        }

        if (parseErrors.Count > 0)
            return Result.Fail(parseErrors);

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.Details.Name,
            request.Details.Description,
            new CspInstance(variables, constraints)
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateCspProblemResult(problem.Id);
    }
}

public sealed record CreateCspProblemCommand(
    ProblemDetailsDto Details,
    CspInstanceDto Instance
) : IRequest<CreateCspProblemResult>;

public sealed record CreateCspProblemResult(Guid ProblemId);

public sealed class CreateCspProblemValidator : AbstractValidator<CreateCspProblemCommand>
{
    public CreateCspProblemValidator(
        IValidator<ProblemDetailsDto> problemDetailsValidator,
        IValidator<CspInstanceDto> cspInstanceDtoValidator)
    {
        RuleFor(dto => dto.Details)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(cspInstanceDtoValidator);
    }
}