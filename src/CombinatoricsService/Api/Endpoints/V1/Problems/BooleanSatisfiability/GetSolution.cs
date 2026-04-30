using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class GetBooleanSatisfiabilitySolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat/solution", Execute)
            .WithName("get boolean satisfiability solution")
            .WithTags("sat")
            .Produces<GetBooleanSatisfiabilitySolutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSatisfiabilitySolutionResult> result = await mediator.Send(
            new GetBooleanSatisfiabilitySolutionQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBooleanSatisfiabilitySolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability))
            : result.ToProblemResult();
    }
}

public sealed record GetBooleanSatisfiabilitySolutionResponse(
    BooleanSatisfiabilitySolutionDto? Solution,
    Satisfiability Satisfiability
);
