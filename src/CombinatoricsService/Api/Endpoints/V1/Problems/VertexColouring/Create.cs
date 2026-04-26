using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
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
            .WithTags("vertex-coloring");
    }

    public static async Task<Results<
        Created<CreateVertexColoringProblemResponse>,
        ValidationProblem,
        InternalServerError
    >> Execute(
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

        if (result.IsSuccess)
            return TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateVertexColoringProblemResponse(result.Value.ProblemId));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class CreateVertexColoringProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public VertexColoringInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateVertexColoringProblemResponse(Guid ProblemId);