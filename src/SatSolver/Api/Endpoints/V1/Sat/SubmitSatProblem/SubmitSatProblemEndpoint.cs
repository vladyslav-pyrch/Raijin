using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.SatSolver.Api.Extensions;
using Raijin.SatSolver.Application.Errors;
using Raijin.SatSolver.Application.Features.SubmitSatProblem;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Api.Endpoints.V1.Sat.SubmitSatProblem;

public class SubmitSatProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/v1/sat", Execute)
            .WithName("SubmitSatProblem")
            .WithTags("sat");
    }

    private static async Task<Results<Ok<SubmitSatProblemResponse>, ValidationProblem, InternalServerError>> Execute(
        [FromBody] SubmitSatProblemRequest request,
        [FromServices] IMediator mediator,
        [FromServices] ILogger<SubmitSatProblemEndpoint> logger,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Received SubmitSatProblem request");

        Result<SubmitSatProblemResult> result = await mediator.Send(new SubmitSatProblemCommand(
            request.Dimacs
        ), cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("SubmitSatProblem succeeded, SatProblemId: {SatProblemId}", result.Value.SatProblemId);
            return TypedResults.Ok(new SubmitSatProblemResponse
            {
                SatProblemId = result.Value.SatProblemId
            });
        }

        if (result.HasError<ValidationError>())
        {
            logger.LogWarning("SubmitSatProblem validation failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Message)));
            return TypedResults.ValidationProblem(errors: result.ToValidationErrorDictionary());
        }

        logger.LogError("SubmitSatProblem failed with unexpected error: {Errors}", string.Join("; ", result.Errors.Select(e => e.Message)));
        return TypedResults.InternalServerError();
    }
}