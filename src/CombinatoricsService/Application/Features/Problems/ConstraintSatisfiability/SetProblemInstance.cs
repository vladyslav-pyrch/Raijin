using System.Text.RegularExpressions;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed record SetCspProblemInstanceCommand(
    Guid ProblemId,
    CspInstanceDto Instance
) : IRequest;

public sealed class SetCspProblemInstanceHandler(
    IBoolExprParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetCspProblemInstanceCommand>
{
    public async Task<Result> Handle(
        SetCspProblemInstanceCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus == SolvingStatus.Running)
            return new ConflictError("Cannot change instance while solving is in progress.");

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

        int constraintIndex = 0;
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

        problem.SetInstance(new CspInstance(variables, constraints));

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record CspInstanceDto(
    IEnumerable<DecisionVariableDto> Variables,
    IEnumerable<string> Constraints
);

public sealed record DecisionVariableDto(string Name, IEnumerable<string> States);

public sealed class CspInstanceDtoValidator : AbstractValidator<CspInstanceDto>
{
    private static readonly Regex VariableNameRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    private static readonly Regex SimpleIdentifierRegex = new(
        @"^[a-zA-Z0-9]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    public CspInstanceDtoValidator()
    {
        RuleFor(dto => dto.Variables)
            .NotNull()
            .WithMessage("Variables must not be null.");

        RuleFor(dto => dto.Constraints)
            .NotNull()
            .WithMessage("Constraints must not be null.");

        RuleForEach(dto => dto.Variables)
            .ChildRules(variable =>
            {
                variable.RuleFor(v => v.Name)
                    .NotEmpty()
                    .WithMessage("Variable name must not be empty.")
                    .Must(name => VariableNameRegex.IsMatch(name))
                    .WithMessage(
                        "Variable name must be a valid identifier. " +
                        "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                        "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                        "Names must end with an alphanumeric character.");

                variable.RuleFor(v => v.States)
                    .NotNull()
                    .WithMessage("States must not be null.")
                    .Must(states => states != null && states.Count() >= 2)
                    .WithMessage("Each variable must have at least 2 states.");

                variable.RuleForEach(v => v.States)
                    .NotEmpty()
                    .WithMessage("State name must not be empty.")
                    .Must(state => state != null && SimpleIdentifierRegex.IsMatch(state))
                    .WithMessage("State names must be simple alphanumeric identifiers (letters and digits only).");
            });
    }
}

public sealed class SetCspProblemInstanceValidator : AbstractValidator<SetCspProblemInstanceCommand>
{
    public SetCspProblemInstanceValidator()
    {
        RuleFor(c => c.ProblemId).NotEmpty();
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(new CspInstanceDtoValidator());
    }
}
