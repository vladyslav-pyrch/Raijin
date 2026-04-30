using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class GetVertexColoringInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/vertex-coloring/instance", Execute)
            .WithName("get vertex coloring instance")
            .WithTags("vertex-coloring")
            .Produces<GetVertexColoringInstanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetVertexColoringInstanceResult> result =
            await mediator.Send(new GetVertexColoringInstanceQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetVertexColoringInstanceResponse(result.Value.Instance))
            : result.ToProblemResult();
    }
}

public sealed record GetVertexColoringInstanceResponse(VertexColoringInstanceDto Instance);
