using FluentValidation;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

public sealed class SolveSatProblemRequestValidator : AbstractValidator<SolveSatProblemRequest>
{
    public SolveSatProblemRequestValidator()
    {
        RuleFor(request => request.Clauses)
            .NotEmpty()
            .WithMessage("Clauses must not be empty.");

        RuleForEach(request => request.Clauses)
            .SetValidator(new ClauseRequestValidator());
    }
}