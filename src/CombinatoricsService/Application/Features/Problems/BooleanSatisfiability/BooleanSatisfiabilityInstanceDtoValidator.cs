using FluentValidation;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

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
                "Separator types ('-', '_', ':') cannot be mixed within a single run. " +
                "Names must end with an alphanumeric character.");
    }
}