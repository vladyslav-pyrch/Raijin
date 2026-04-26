using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class SolveProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/problems/{id:Guid}/solve", Execute)
            .WithName("reduce to sat")
            .WithTags("problems");
    }

    public static async Task<Results<
        NoContent,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromRoute] Guid id,
        [FromQuery] string solver,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<SolveProblemResult> result = await mediator.Send(
            new SolveProblemCommand(id, solver), cancellationToken);

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
