using FluentValidation;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

public class SolveSatRequestValidator : AbstractValidator<SolveSatRequest>
{
    public SolveSatRequestValidator()
    {
        RuleFor(request => request.Dimacs).NotEmpty();
    }
}