using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetSatEncodingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat-encoding", Execute)
            .WithName("get sat encoding")
            .WithTags("problems", "sat-encoding")
            .Produces<GetSatEncodingResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetSatEncodingResult> result = await mediator.Send(new GetSatEncodingQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetSatEncodingResponse(
                result.Value.NumberOfVariables,
                result.Value.NumberOfClauses,
                result.Value.Clauses))
            : result.ToProblemResult();
    }
}

public sealed record GetSatEncodingResponse(
    int NumberOfVariables,
    int NumberOfClauses,
    IReadOnlyList<IReadOnlyList<int>> Clauses
);