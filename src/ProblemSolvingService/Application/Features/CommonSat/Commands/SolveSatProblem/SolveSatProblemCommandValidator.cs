using FluentValidation;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandValidator : AbstractValidator<SolveSatProblemCommand>
{
    public SolveSatProblemCommandValidator()
    {
        RuleFor(command => command.Clauses)
            .NotEmpty()
            .WithMessage("The list of clauses must not be empty.");

        RuleForEach(command => command.Clauses)
            .SetValidator(new ClauseDtoValidator());
    }
}