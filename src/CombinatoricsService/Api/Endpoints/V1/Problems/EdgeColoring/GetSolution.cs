using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class GetEdgeColoringSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/edge-coloring/solution", Execute)
            .WithName("get edge coloring solution")
            .WithTags("edge-coloring")
            .Produces<GetEdgeColoringSolutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetEdgeColoringSolutionResult> result = await mediator.Send(
            new GetEdgeColoringSolutionQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetEdgeColoringSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability))
            : result.ToProblemResult();
    }
}

public sealed record GetEdgeColoringSolutionResponse(
    EdgeColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);