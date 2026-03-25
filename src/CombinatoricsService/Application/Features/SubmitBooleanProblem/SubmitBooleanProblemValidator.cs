using FluentValidation;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public class SubmitBooleanProblemValidator : AbstractValidator<SubmitBooleanProblemCommand>
{
    public SubmitBooleanProblemValidator()
    {
        RuleFor(command => command.BooleanFormula)
            .NotEmpty()
            .WithMessage("Boolean expression must not be empty.")
            .Custom((booleanFormula, context) =>
            {
                try
                {
                    _ = ExpressionParser.Parse(booleanFormula);
                }
                catch (Exception ex)
                {
                    context.AddFailure($"The boolean formula is not valid: {ex.Message}");
                }
            });
    }
}