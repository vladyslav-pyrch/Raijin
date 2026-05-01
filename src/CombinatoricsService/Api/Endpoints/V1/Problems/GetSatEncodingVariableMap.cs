using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetSatEncodingVariableMapEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat-encoding/variable-map", Execute)
            .WithName("get sat encoding variable map")
            .WithTags("problems", "sat-encoding")
            .Produces<GetSatEncodingVariableMapResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetSatEncodingVariableMapResult> result = await mediator.Send(new GetSatEncodingVariableMapQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetSatEncodingVariableMapResponse(
                result.Value.Variables
                    .Select(v => new VariableMapEntryResponse(v.Name, v.Index))
                    .ToList()))
            : result.ToProblemResult();
    }
}

public sealed record GetSatEncodingVariableMapResponse(IReadOnlyList<VariableMapEntryResponse> Variables);

public sealed record VariableMapEntryResponse(string Name, int Index);