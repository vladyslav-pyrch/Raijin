using FluentValidation;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemValidator : AbstractValidator<SolveSatProblemCommand>
{
    public SolveSatProblemValidator()
    {
        RuleFor(x => x.SatProblemId)
            .NotEmpty();
    }
}