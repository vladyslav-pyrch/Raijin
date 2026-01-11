using FluentValidation;
using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemValidator : AbstractValidator<SolveSatProblemCommand>
{
    public SolveSatProblemValidator()
    {
        RuleFor(command => command.Clauses)
            .NotEmpty()
            .WithMessage("The list of clauses must not be empty.");

        RuleForEach(command => command.Clauses)
            .SetValidator(new ClauseDtoValidator());
    }
}