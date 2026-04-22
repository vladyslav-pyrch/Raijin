using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class GetEdgeColoringSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/solution/edge-coloring", Execute)
            .WithName("get edge coloring solution")
            .WithTags("problems", "edge-coloring");
    }

    public static async Task<Results<
        Ok<GetEdgeColoringSolutionResponse>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ValidationProblem,
        InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetEdgeColoringSolutionResult> result = await mediator.Send(
            new GetEdgeColoringSolutionQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetEdgeColoringSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability));

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        if (result.Has(out DomainError? domainError))
            return domainError.ToUnprocessableEntityResult();

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetEdgeColoringSolutionResponse(
    EdgeColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);
