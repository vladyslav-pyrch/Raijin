using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class ResolveCombinatoricProblemValidator : AbstractValidator<ResolveCombinatoricProblemCommand>
{
    public ResolveCombinatoricProblemValidator(IValidator<BooleanProblemSolutionDto> booleanProblemSolutionValidator)
    {
        RuleFor(x => x.BooleanProblemSolutionSolution)
            .NotNull()
            .WithMessage("Boolean problem solution cannot be null.")
            .SetValidator(booleanProblemSolutionValidator);
    }
}