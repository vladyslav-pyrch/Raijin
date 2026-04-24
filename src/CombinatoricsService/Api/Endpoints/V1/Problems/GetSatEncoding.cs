using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetSatEncodingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat-encoding", Execute)
            .WithName("get sat encoding")
            .WithTags("problems", "sat-encoding");
    }

    public static async Task<Results<Ok<GetSatEncodingResponse>, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetSatEncodingResult> result = await mediator.Send(new GetSatEncodingQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetSatEncodingResponse(
                result.Value.NumberOfVariables,
                result.Value.NumberOfClauses,
                result.Value.Clauses));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetSatEncodingResponse(
    int NumberOfVariables,
    int NumberOfClauses,
    IReadOnlyList<IReadOnlyList<int>> Clauses
);
