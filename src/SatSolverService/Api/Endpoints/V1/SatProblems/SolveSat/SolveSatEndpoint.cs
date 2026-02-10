using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

public static class SolveSatEndpoint
{
    public static async Task<Results<Ok<SolveSatResponse>, ValidationProblem>> Execute(
        [FromBody] SolveSatRequest request,
        [FromServices] SolveSatCommandHandler handler,
        [FromServices] IValidator<SolveSatRequest> validator,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        Result<int> satIdResult = await handler.Handle(new SolveSatCommand(request.Dimacs), cancellationToken);

        if (satIdResult.IsSuccess)
            return TypedResults.Ok(new SolveSatResponse(satIdResult.Value));

        return TypedResults.ValidationProblem([
            new KeyValuePair<string, string[]>(
                nameof(request.Dimacs),
                satIdResult.Errors.Select(error => error.Message).ToArray()
            )
        ]);
    }
}