using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class GetBooleanSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/bool/solution", Execute)
            .WithName("get boolean solution")
            .WithTags("bool")
            .Produces<GetBooleanSolutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSolutionResult> result = await mediator.Send(
            new GetBooleanSolutionQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBooleanSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability))
            : result.ToProblemResult();
    }
}

public sealed record GetBooleanSolutionResponse(
    BooleanSolutionDto? Solution,
    Satisfiability Satisfiability
);
