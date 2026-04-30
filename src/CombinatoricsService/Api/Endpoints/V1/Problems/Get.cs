using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}", Execute)
            .WithName("get problem")
            .WithTags("problems")
            .Produces<GetProblemResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetProblemResult> result = await mediator.Send(new GetProblemQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetProblemResponse(
                result.Value.Id,
                result.Value.Name,
                result.Value.Description,
                result.Value.Solver,
                result.Value.InstanceType,
                result.Value.SolvingStatus.ToString(),
                result.Value.Satisfiability.ToString(),
                result.Value.CreatedAt,
                result.Value.UpdatedAt,
                result.Value.CompletedAt))
            : result.ToProblemResult();
    }
}

public sealed record GetProblemResponse(
    Guid Id,
    string Name,
    string Description,
    string? Solver,
    string InstanceType,
    string SolvingStatus,
    string Satisfiability,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
);
