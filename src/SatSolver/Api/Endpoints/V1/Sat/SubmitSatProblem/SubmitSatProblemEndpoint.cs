using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.SatSolver.Application.Cqrs;
using Raijin.SatSolver.Application.Features.SubmitSatProblem;

namespace Raijin.SatSolver.Api.Endpoints.V1.Sat.SubmitSatProblem;

public class SubmitSatProblemEndpoint : IEndpoint
{
    public void Map(WebApplication app)
    {
        app.MapPost("/sat", Execute)
            .WithName("SubmitSatProblem")
            .WithTags("sat");
    }

    private static async Task<Results<Ok<SubmitSatProblemResponse>, ValidationProblem, BadRequest>> Execute(
        [FromBody] SubmitSatProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<SubmitSatProblemResult> result =
            await mediator.Send(new SubmitSatProblemCommand(request.Dimacs), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new SubmitSatProblemResponse
            {
                SatProblemId = result.Value.SatProblemId
            });

        if (!result.HasError<DimacsFormatValidationError>())
            return TypedResults.BadRequest();

        string[] errors = result.Errors
            .OfType<DimacsFormatValidationError>()
            .Select(error => error.Message)
            .ToArray();

        return TypedResults.ValidationProblem(errors:
            [new KeyValuePair<string, string[]>(nameof(request.Dimacs), errors)]
        );
    }
}