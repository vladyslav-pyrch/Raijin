using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class CreateProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/problems", Execute)
            .WithName("create problem")
            .WithTags("problems");
    }

    public static async Task<Results<Created<CreateProblemResponse>, ValidationProblem, InternalServerError>> Execute(
        [FromBody] CreateProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateProblemResult> result = await mediator.Send(new CreateProblemCommand(
            request.Name,
            request.Description ?? string.Empty
        ), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Created($"/problems/{result.Value.Id}", new CreateProblemResponse(result.Value.Id));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class CreateProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

public sealed record CreateProblemResponse(Guid ProblemId);