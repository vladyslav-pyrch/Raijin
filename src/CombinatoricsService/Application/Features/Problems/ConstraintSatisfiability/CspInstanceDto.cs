using System.Text.RegularExpressions;
using FluentValidation;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

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
        TimeSpan.FromMilliseconds(100));

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
                    .Must(states => states.Count() >= 2)
                    .WithMessage("Each variable must have at least 2 states.");

                variable.RuleForEach(v => v.States)
                    .NotEmpty()
                    .WithMessage("State name must not be empty.")
                    .Must(state => VariableNameRegex.IsMatch(state))
                    .WithMessage(
                        "State names must be a valid identifier. " +
                        "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                        "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                        "Names must end with an alphanumeric character.");
            });
    }
}