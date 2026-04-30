using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class UpdateProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPatch("problems/{id:Guid}", Execute)
            .WithName("update problem")
            .WithTags("problems")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromBody] UpdateProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result result = await mediator.Send(new UpdateProblemCommand(id, request.Name, request.Description),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemResult();
    }
}

public sealed class UpdateProblemRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}
