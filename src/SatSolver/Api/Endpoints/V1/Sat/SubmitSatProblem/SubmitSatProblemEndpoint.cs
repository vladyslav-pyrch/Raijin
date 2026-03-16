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
        CancellationToken cancellationToken
    )
    {
        Result<SubmitSatProblemResult> result =
            await mediator.Send(request.ToCommand(), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new SubmitSatProblemResponse
            {
                SatProblemId = result.Value.SatProblemId
            });

        if (result.HasError<ValidationError>())
            return TypedResults.ValidationProblem(errors: result.ToValidationErrorDictionary());

        return TypedResults.InternalServerError();
    }
}