using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class GetVertexColoringInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/vertex-coloring/instance", Execute)
            .WithName("get vertex coloring instance")
            .WithTags("vertex-coloring");
    }

    public static async Task<Results<Ok<GetVertexColoringInstanceResponse>, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetVertexColoringInstanceResult> result =
            await mediator.Send(new GetVertexColoringInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetVertexColoringInstanceResponse(result.Value.Instance));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetVertexColoringInstanceResponse(VertexColoringInstanceDto Instance);
