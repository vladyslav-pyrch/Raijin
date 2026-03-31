using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class SetProblemInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPut("problems/{id:Guid}/instance", Execute)
            .WithName("set problem instance")
            .WithTags("problems");
    }

    public static async Task<Results<NoContent, ValidationProblem, NotFound, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromBody] SetProblemInstanceRequest request,
        [FromServices] IMediator mediator
    )
    {
        Result result = await mediator.Send(new SetProblemInstanceCommand(
            id,
            request.Instance
        ), CancellationToken.None);

        if (result.IsNotFoundError())
            return TypedResults.NotFound();

        if (result.IsValidationError())
            return TypedResults.ValidationProblem(result.ToValidationErrorDictionary());

        if (result.IsFailed)
            return TypedResults.InternalServerError();

        return TypedResults.NoContent();
    }
}

public sealed class SetProblemInstanceRequest
{
    public InstanceDto Instance { get; set; } = null!;
}