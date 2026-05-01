using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class SolveProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/problems/{id:Guid}/solve", Execute)
            .WithName("reduce to sat")
            .WithTags("problems")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromQuery] string solver,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<SolveProblemResult> result = await mediator.Send(
            new SolveProblemCommand(id, solver), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemResult();
    }
}