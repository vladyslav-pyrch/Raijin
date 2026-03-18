using FluentValidation;
using Raijin.SatSolver.Application.Validation;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed class SubmitSatProblemValidator : AbstractValidator<SubmitSatProblemCommand>
{
    public SubmitSatProblemValidator()
    {
        RuleFor(command => command.Dimacs)
            .NotEmpty()
            .MustBeDimacs();
    }
}