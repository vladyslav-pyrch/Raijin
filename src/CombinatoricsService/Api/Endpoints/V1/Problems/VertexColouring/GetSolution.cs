using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class GetVertexColoringSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/vertex-coloring/solution", Execute)
            .WithName("get vertex coloring solution")
            .WithTags("vertex-coloring")
            .Produces<GetVertexColoringSolutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetVertexColoringSolutionResult> result = await mediator.Send(
            new GetVertexColoringSolutionQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetVertexColoringSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability))
            : result.ToProblemResult();
    }
}

public sealed record GetVertexColoringSolutionResponse(
    VertexColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);
