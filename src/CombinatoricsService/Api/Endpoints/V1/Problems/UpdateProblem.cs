using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public class UpdateProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPatch("problems/{id:Guid}", Execute)
            .WithName("update problem")
            .WithTags("problems");
    }

    public static async Task<Results<NoContent, ValidationProblem, NotFound, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromBody] UpdateProblemRequest request,
        [FromServices] IMediator mediator
    )
    {
        Result result = await mediator.Send(new UpdateProblemCommand(
            id,
            request.Name,
            request.Description,
            request.ProblemType,
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

public class UpdateProblemRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ProblemType { get; set; }

    public InstanceDto? Instance { get; set; }
}