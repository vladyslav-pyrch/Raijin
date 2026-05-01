using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class GetBooleanSatisfiabilityInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat/instance", Execute)
            .WithName("get boolean satisfiability instance")
            .WithTags("sat")
            .Produces<GetBooleanSatisfiabilityInstanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSatisfiabilityInstanceResult> result =
            await mediator.Send(new GetBooleanSatisfiabilityInstanceQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBooleanSatisfiabilityInstanceResponse(result.Value.Instance))
            : result.ToProblemResult();
    }
}

public sealed record GetBooleanSatisfiabilityInstanceResponse(SatInstanceDto Instance);