using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public sealed class SatSolutionDtoValidator : AbstractValidator<SatSolutionDto>
{
    public SatSolutionDtoValidator()
    {
        RuleFor(solution => solution.Literals)
            .NotNull()
            .WithMessage("Literals cannot be null.")
            .Custom((literals, context) =>
            {
                List<(int Literal, int Expected)> wrongLiterals = literals.Select(Math.Abs)
                    .Order()
                    .Select((literal, index) => (Litteral: literal, Expected: index + 1))
                    .Where(item => item.Litteral != item.Expected)
                    .ToList();


                if (wrongLiterals.Any())
                    context.AddFailure("The SAT solution variables must form a contiguous sequence starting at 1.");

                foreach ((int literal, int expected) in wrongLiterals)
                    context.AddFailure("A SAT solution literal must not be zero.");
            });

        RuleForEach(solution => solution.Literals)
            .Must(literal => literal != 0)
            .WithMessage("A SAT solution literal must not be zero.");
    }
}