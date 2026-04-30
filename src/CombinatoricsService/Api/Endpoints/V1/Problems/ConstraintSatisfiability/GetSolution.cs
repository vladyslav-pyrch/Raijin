using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class GetCspSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/csp/solution", Execute)
            .WithName("get csp solution")
            .WithTags("csp")
            .Produces<GetCspSolutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetCspSolutionResult> result = await mediator.Send(
            new GetCspSolutionQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCspSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability))
            : result.ToProblemResult();
    }
}

public sealed record GetCspSolutionResponse(
    CspSolutionDto? Solution,
    Satisfiability Satisfiability
);
