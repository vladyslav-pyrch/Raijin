using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class GetBooleanSatisfiabilityInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat/instance", Execute)
            .WithName("get boolean satisfiability instance")
            .WithTags("sat");
    }

    public static async Task<Results<Ok<GetBooleanSatisfiabilityInstanceResponse>, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanSatisfiabilityInstanceResult> result =
            await mediator.Send(new GetBooleanSatisfiabilityInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetBooleanSatisfiabilityInstanceResponse(result.Value.Instance));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetBooleanSatisfiabilityInstanceResponse(SatInstanceDto Instance);
