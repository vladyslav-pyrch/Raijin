using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed class CspSetProblemInstance(
    IBoolExprParser parser,
    IValidator<CspInstanceDto>? validator = null
) : ISetProblemInstanceExtension
{
    public string ProblemType => ProblemTypes.ConstraintSatisfiabilityProblem;

    public Result<Instance> CreateInstance(InstanceDto instanceDto)
    {
        if (instanceDto is not CspInstanceDto cspDto)
            throw new ArgumentException(
                $"Invalid instance DTO type. Expected {typeof(CspInstanceDto)}, got {instanceDto.GetType()}");

        var validationResult = Result.FailIfNotEmpty(
            validator?.Validate(cspDto).ToValidationErrors() ?? []
        );

        if (validationResult.IsFailed)
            return validationResult;

        // Check for duplicate variable names
        List<string> variableNames = cspDto.Variables.Select(v => v.Name).ToList();
        IEnumerable<string> duplicates = variableNames
            .GroupBy(name => name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        List<ValidationError> duplicateErrors = duplicates
            .Select(name => new ValidationError(
                nameof(CspInstanceDto.Variables),
                $"Duplicate variable name: '{name}'."))
            .ToList();

        if (duplicateErrors.Count > 0)
            return Result.Fail(duplicateErrors);

        // Build domain variables
        List<DecisionVariable> variables = cspDto.Variables
            .Select(v => new DecisionVariable(v.Name, v.States.ToList()))
            .ToList();

        // Parse constraint formulas
        List<BoolExpr> constraints = [];
        List<IError> parseErrors = [];

        int constraintIndex = 0;
        foreach (string constraintFormula in cspDto.Constraints)
        {
            Result<BoolExpr> parseResult = parser.Parse(constraintFormula);
            if (parseResult.IsFailed)
                parseErrors.AddRange(parseResult.Errors.Select(e =>
                    new ValidationError(
                        $"{nameof(CspInstanceDto.Constraints)}[{constraintIndex}]",
                        e.Message)
                    )
                );
            else
                constraints.Add(parseResult.Value);

            constraintIndex++;
        }

        if (parseErrors.Count > 0)
            return Result.Fail(parseErrors);

        var cspInstance = new CspInstance(variables, constraints);
        return Result.Ok<Instance>(cspInstance);
    }
}

public record CspInstanceDto(
    IEnumerable<DecisionVariableDto> Variables,
    IEnumerable<string> Constraints
) : InstanceDto
{
    [JsonIgnore] public override string ProblemType => ProblemTypes.ConstraintSatisfiabilityProblem;
}

public record DecisionVariableDto(string Name, IEnumerable<string> States);

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