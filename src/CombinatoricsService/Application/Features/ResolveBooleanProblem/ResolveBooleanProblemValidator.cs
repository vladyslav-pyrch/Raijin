using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public sealed class ResolveBooleanProblemValidator : AbstractValidator<ResolveBooleanProblemCommand>
{
    public ResolveBooleanProblemValidator(IValidator<SatSolutionDto> satSolutionDtoValidator)
    {
        RuleFor(command => command.SatSolution)
            .NotNull()
            .WithMessage("SAT solution cannot be null.")
            .SetValidator(satSolutionDtoValidator);
    }
}