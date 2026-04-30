using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class GetBooleanInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/bool/instance", Execute)
            .WithName("get boolean instance")
            .WithTags("bool")
            .Produces<GetBooleanInstanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanInstanceResult> result =
            await mediator.Send(new GetBooleanInstanceQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBooleanInstanceResponse(result.Value.Instance))
            : result.ToProblemResult();
    }
}

public sealed record GetBooleanInstanceResponse(BooleanProblemInstanceDto Instance);
