using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class UpdateProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPatch("problems/{id:Guid}", Execute)
            .WithName("update problem")
            .WithTags("problems");
    }

    public static async Task<Results<NoContent, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromBody] UpdateProblemRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result result = await mediator.Send(new UpdateProblemCommand(id, request.Name, request.Description),
            cancellationToken);

        if (result.IsSuccess)
            return TypedResults.NoContent();

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class UpdateProblemRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}