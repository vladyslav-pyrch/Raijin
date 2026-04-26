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
        endpoint.MapGet("problems/{id:Guid}/edge-coloring/instance", Execute)
            .WithName("get edge coloring instance")
            .WithTags("edge-coloring");
    }

    public static async Task<Results<
        Ok<GetEdgeColoringInstanceResponse>,
        NotFound<ProblemDetails>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetEdgeColoringInstanceResult> result =
            await mediator.Send(new GetEdgeColoringInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetEdgeColoringInstanceResponse(result.Value.Instance));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public record GetEdgeColoringInstanceResponse(EdgeColoringInstanceDto Instance);
