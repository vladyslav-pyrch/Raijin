using FluentValidation;
using Raijin.CombinatoricsService.Domain.Problems;

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
            .Must(clause => clause.All(literal => literal != 0))
            .WithMessage("Literals must be non-zero integers.");
    }

    public string ProblemType => ProblemTypes.BooleanSatisfiabilityProblem;
}