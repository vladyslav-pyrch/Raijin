using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColouring;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class GetVertexColoringSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/solution/vertex-coloring", Execute)
            .WithName("get vertex coloring solution")
            .WithTags("problems", "vertex-coloring");
    }

    public static async Task<Results<
        Ok<GetVertexColoringSolutionResponse>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ValidationProblem,
        InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetVertexColoringSolutionResult> result = await mediator.Send(
            new GetVertexColoringSolutionQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetVertexColoringSolutionResponse(
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

public sealed record GetVertexColoringSolutionResponse(
    VertexColoringSolutionDto? Solution,
    Satisfiability Satisfiability
);
