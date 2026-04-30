using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class CreateSatProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/sat", Execute)
            .WithName("create sat problem")
            .WithTags("sat")
            .Produces<CreateSatProblemResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> Execute(
        [FromBody] CreateSatProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateSatProblemResult> result = await mediator.Send(new CreateSatProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateSatProblemResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }
}

public sealed class CreateSatProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public SatInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateSatProblemResponse(Guid ProblemId);
