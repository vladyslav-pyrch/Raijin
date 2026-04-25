using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class GetBooleanSolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/solution/bool", Execute)
            .WithName("get boolean solution")
            .WithTags("problems", "bool");
    }

    public static async Task<Results<
        Ok<GetBooleanSolutionResponse>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ValidationProblem,
        InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSolutionResult> result = await mediator.Send(
            new GetBooleanSolutionQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetBooleanSolutionResponse(
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

public sealed record GetBooleanSolutionResponse(
    BooleanSolutionDto? Solution,
    Satisfiability Satisfiability
);
