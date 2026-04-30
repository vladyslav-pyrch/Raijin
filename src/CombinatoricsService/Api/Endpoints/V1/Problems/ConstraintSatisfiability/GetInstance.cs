using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class GetCspInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/csp/instance", Execute)
            .WithName("get csp instance")
            .WithTags("csp")
            .Produces<GetCspInstanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetCspInstanceResult> result =
            await mediator.Send(new GetCspInstanceQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCspInstanceResponse(result.Value.Instance))
            : result.ToProblemResult();
    }
}

public sealed record GetCspInstanceResponse(CspInstanceDto Instance);

public sealed record CspVariableResponse(string Name, IReadOnlyList<string> States);
