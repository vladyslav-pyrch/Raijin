using FluentValidation;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

public sealed class LiteralDtoValidator : AbstractValidator<LiteralDto>
{
    public LiteralDtoValidator()
    {
        RuleFor(literal => literal.VariableNumber)
            .GreaterThan(0)
            .WithMessage("Variable number must be greater than 0.");
    }
}