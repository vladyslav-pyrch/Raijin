using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class GetCspSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/csp/solution", Execute)
            .WithName("get csp solution")
            .WithTags("csp");
    }

    public static async Task<Results<
        Ok<GetCspSolutionResponse>,
        NotFound<ProblemDetails>,
        ValidationProblem,
        InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetCspSolutionResult> result = await mediator.Send(
            new GetCspSolutionQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetCspSolutionResponse(
                result.Value.Solution,
                result.Value.Satisfiability));

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetCspSolutionResponse(
    CspSolutionDto? Solution,
    Satisfiability Satisfiability
);
