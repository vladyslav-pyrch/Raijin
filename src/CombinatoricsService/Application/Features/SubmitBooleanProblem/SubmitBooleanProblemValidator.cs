using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public class SubmitBooleanProblemValidator : AbstractValidator<SubmitBooleanProblemCommand>
{
    public SubmitBooleanProblemValidator()
    {
        RuleFor(command => command.BooleanFormula)
            .NotEmpty()
            .WithMessage("Boolean expression must not be empty.");
    }
}