using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
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

    public static async Task<Results<NoContent, ValidationProblem, NotFound, Conflict<string>, InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator
        )
    {
        Result result = await mediator.Send(new ReduceProblemToSatCommand(
            id
        ), CancellationToken.None);

        if (result.IsValidationError())
            return TypedResults.ValidationProblem(result.ToValidationErrorDictionary());

        if (result.IsNotFoundError())
            return TypedResults.NotFound();

        if (result.IsIllegalOperationError())
            return TypedResults.Conflict(result.Errors.First().Message);

        if (result.IsFailed)
            return TypedResults.InternalServerError();

        return TypedResults.NoContent();
    }
}