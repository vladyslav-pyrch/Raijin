using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class GetEdgeColoringInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/instance/edge-coloring", Execute)
            .WithName("get edge coloring instance")
            .WithTags("problems", "edge-coloring");
    }

    public static async Task<Results<Ok<GetEdgeColoringInstanceResponse>, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetEdgeColoringInstanceResult> result =
            await mediator.Send(new GetEdgeColoringInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
        {
            var value = result.Value;
            return TypedResults.Ok(new GetEdgeColoringInstanceResponse(
                value.Vertices,
                value.Edges.Select(e => new EdgeResponse(e.Label, e.U, e.V)).ToList(),
                value.ColorCount));
        }

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetEdgeColoringInstanceResponse(
    IReadOnlyList<string> Vertices,
    IReadOnlyList<EdgeResponse> Edges,
    int ColorCount
);

public sealed record EdgeResponse(string Label, string U, string V);
