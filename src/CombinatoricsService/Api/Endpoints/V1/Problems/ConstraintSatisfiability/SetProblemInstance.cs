using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class SetCspProblemInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/{id:Guid}/instance/csp", Execute)
            .WithName("set csp problem instance")
            .WithTags("problems", "csp");
    }

    public static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>, ValidationProblem, InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromBody] SetCspProblemInstanceRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result result = await mediator.Send(
            new SetCspProblemInstanceCommand(id, request.Instance),
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

public sealed class SetCspProblemInstanceRequest
{
    public CspInstanceDto Instance { get; set; } = null!;
}
