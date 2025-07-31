using FluentValidation;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

public class ClauseDtoValidator : AbstractValidator<ClauseDto>
{
    public ClauseDtoValidator()
    {
        RuleFor(clause => clause.Literals)
            .NotEmpty()
            .WithMessage("The list of literals in a clause must not be empty.");

        RuleForEach(clause => clause.Literals)
            .SetValidator(new LiteralDtoValidator());
    }
}