using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class SetBooleanSatisfiabilityProblemInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/{id:Guid}/instance/sat", Execute)
            .WithName("set boolean satisfiability problem instance")
            .WithTags("problems", "sat");
    }

    public static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>, ValidationProblem, InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromBody] SetBooleanSatisfiabilityProblemInstanceRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result result = await mediator.Send(
            new SetBooleanSatisfiabilityProblemInstanceCommand(id, request.Instance),
            cancellationToken);

        if (result.IsSuccess)
            return TypedResults.NoContent();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        if (result.Has(out ConflictError? conflictError))
            return conflictError.ToConflictResult();

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class SetBooleanSatisfiabilityProblemInstanceRequest
{
    public BooleanSatisfiabilityInstanceDto Instance { get; set; } = null!;
}
