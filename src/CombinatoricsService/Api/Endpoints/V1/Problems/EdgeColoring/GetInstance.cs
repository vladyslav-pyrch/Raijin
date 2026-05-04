using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class GetEdgeColoringInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/edge-coloring/instance", Execute)
            .WithName("get edge coloring instance")
            .WithTags("edge-coloring")
            .Produces<GetEdgeColoringInstanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetEdgeColoringInstanceResult> result =
            await mediator.Send(new GetEdgeColoringInstanceQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetEdgeColoringInstanceResponse(result.Value.Instance))
            : result.ToProblemResult();
    }
}

public record GetEdgeColoringInstanceResponse(EdgeColoringInstanceDto Instance);