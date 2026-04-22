using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class GetBooleanSatisfiabilitySolutionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/solution/sat", Execute)
            .WithName("get boolean satisfiability solution")
            .WithTags("problems", "sat");
    }

    public static async Task<Results<
        Ok<GetBooleanSatisfiabilitySolutionResponse>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ValidationProblem,
        InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSatisfiabilitySolutionResult> result = await mediator.Send(
            new GetBooleanSatisfiabilitySolutionQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetBooleanSatisfiabilitySolutionResponse(
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

public sealed record GetBooleanSatisfiabilitySolutionResponse(
    BooleanSatisfiabilitySolutionDto? Solution,
    Satisfiability Satisfiability
);
