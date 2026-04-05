using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class ReduceProblemToSatEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/{id:Guid}/sat-reduction", Execute)
            .WithName("reduce problem to sat")
            .WithTags("problems");
    }

    public static async
        Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>, InternalServerError>> Execute(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result result = await mediator.Send(new ReduceProblemToSatCommand(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.NoContent();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        if (result.Has(out ConflictError? conflictError))
            return conflictError.ToConflictResult();

        return TypedResults.InternalServerError();
    }
}