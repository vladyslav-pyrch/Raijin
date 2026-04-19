using System.Text.Json.Serialization;
using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public class SatSetProblemInstance(
    IValidator<BooleanSatisfiabilityInstanceDto>? validator
) : ISetProblemInstanceExtension
{
    public string ProblemType => ProblemTypes.BooleanSatisfiabilityProblem;

    public Result<Instance> CreateInstance(InstanceDto instanceDto)
    {
        if (instanceDto is not BooleanSatisfiabilityInstanceDto booleanSatisfiabilityInstanceDto)
            throw new ArgumentException(
                $"Invalid instance DTO type. Expected {typeof(BooleanSatisfiabilityInstanceDto)}, got {instanceDto.GetType()}");

        var result = Result.FailIfNotEmpty(
            validator?.Validate(booleanSatisfiabilityInstanceDto).ToValidationErrors() ?? []
        );

        if (result.IsFailed)
            return result;

        IEnumerable<List<Literal>> literals = booleanSatisfiabilityInstanceDto.Clauses.Select(clause => clause
            .Select(literalStr =>
            {
                bool negated = literalStr.StartsWith('~');
                string name = negated ? literalStr[1..] : literalStr;
                return new Literal(new SatVariable(name), negated);
            }).ToList());

        var clauses = literals.Select(clauseLiterals => new Clause(clauseLiterals)).ToList();
        return new BooleanSatisfiabilityInstance(clauses);
    }
}

public record BooleanSatisfiabilityInstanceDto(IEnumerable<IEnumerable<string>> Clauses)
    : InstanceDto
{
    [JsonIgnore] public override string ProblemType => ProblemTypes.BooleanSatisfiabilityProblem;
}

public class BooleanSatisfiabilityInstanceDtoValidator : AbstractValidator<BooleanSatisfiabilityInstanceDto>
{
    public BooleanSatisfiabilityInstanceDtoValidator()
    {
        RuleFor(instance => instance.Clauses)
            .NotNull()
            .WithMessage("Clauses must not be null.");

        RuleForEach(instance => instance.Clauses)
            .NotEmpty()
            .WithMessage("Each clause must contain at least one literal.")
            .Must(clause => clause.All(lit => lit is not null && SatVariable.IsValidLiteralString(lit)))
            .WithMessage(
                "Every literal must be a valid named variable literal. " +
                "Format: optional leading '~' for negation, followed by a variable name. " +
                "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                "Names must end with an alphanumeric character.");
    }
}