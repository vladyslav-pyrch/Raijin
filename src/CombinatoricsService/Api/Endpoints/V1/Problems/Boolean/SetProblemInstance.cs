using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class SetBooleanProblemInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/{id:Guid}/instance/bool", Execute)
            .WithName("set boolean problem instance")
            .WithTags("problems", "bool");
    }

    public static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>, ValidationProblem, InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromBody] SetBooleanProblemInstanceRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result result = await mediator.Send(
            new SetBooleanProblemInstanceCommand(id, request.Instance),
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

public sealed class SetBooleanProblemInstanceRequest
{
    public BooleanProblemInstanceDto Instance { get; set; } = null!;
}
