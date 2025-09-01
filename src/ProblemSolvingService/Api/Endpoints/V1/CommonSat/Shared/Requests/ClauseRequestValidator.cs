using FluentValidation;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;

public sealed class ClauseRequestValidator : AbstractValidator<ClauseRequest>
{
    public ClauseRequestValidator()
    {
        RuleFor(request => request.Literals)
            .NotEmpty()
            .WithMessage("Literals must not be empty.");

        RuleForEach(request => request.Literals)
            .SetValidator(new LiteralRequestValidator());
    }
}