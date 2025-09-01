using FluentValidation;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;

public sealed class LiteralRequestValidator : AbstractValidator<LiteralRequest>
{
    public LiteralRequestValidator()
    {
        RuleFor(request => request.VariableNumber)
            .GreaterThan(0)
            .WithMessage("Variable number must be greater than 0.");
    }
}