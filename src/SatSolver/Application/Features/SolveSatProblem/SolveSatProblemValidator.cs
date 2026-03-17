using FluentValidation;
using Raijin.SatSolver.Application.Validation;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemValidator : AbstractValidator<SolveSatProblemCommand>
{
    public SolveSatProblemValidator()
    {
        RuleFor(x => x.SatProblemId)
            .NotEmpty();

        RuleFor(x => x.Dimacs)
            .MustBeDimacs();
    }
}