using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class CreateEdgeColoringProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/edge-coloring", Execute)
            .WithName("create edge coloring problem")
            .WithTags("edge-coloring");
    }

    public static async Task<Results<
        Created<CreateEdgeColoringProblemResponse>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromBody] CreateEdgeColoringProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateEdgeColoringProblemResult> result = await mediator.Send(new CreateEdgeColoringProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateEdgeColoringProblemResponse(result.Value.ProblemId));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class CreateEdgeColoringProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public EdgeColoringInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateEdgeColoringProblemResponse(Guid ProblemId);