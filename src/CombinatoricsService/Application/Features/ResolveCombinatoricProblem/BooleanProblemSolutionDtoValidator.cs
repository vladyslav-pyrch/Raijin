using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class BooleanProblemSolutionDtoValidator : AbstractValidator<BooleanProblemSolutionDto>
{
    public BooleanProblemSolutionDtoValidator()
    {
        RuleFor(x => x.VariableAssignments)
            .NotNull()
            .WithMessage("Variable assignments cannot be null.");
    }
}