using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class CreateVertexColoringProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/vertex-coloring", Execute)
            .WithName("create vertex coloring problem")
            .WithTags("vertex-coloring")
            .Produces<CreateVertexColoringProblemResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> Execute(
        [FromBody] CreateVertexColoringProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateVertexColoringProblemResult> result = await mediator.Send(new CreateVertexColoringProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateVertexColoringProblemResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }
}

public sealed class CreateVertexColoringProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public VertexColoringInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateVertexColoringProblemResponse(Guid ProblemId);
