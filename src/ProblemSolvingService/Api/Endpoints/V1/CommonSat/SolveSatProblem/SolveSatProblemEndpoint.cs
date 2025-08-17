using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

public static class SolveSatProblemEndpoint
{
    public static async Task<Results<Ok<SolveSatProblemResponse>, ValidationProblem>> Execute(
        [FromBody] SolveSatProblemRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await new SolveSatProblemRequestValidator().ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        SolveSatProblemCommand command = request.ToSolveStaProblemCommand();

        SolveSatProblemCommandResult result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(SolveSatProblemResponse.FromSolveSatProblemCommandResult(result));
    }
}