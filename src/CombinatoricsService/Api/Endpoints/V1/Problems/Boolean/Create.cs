using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class CreateBooleanProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/bool", Execute)
            .WithName("create boolean problem")
            .WithTags("bool");
    }

    public static async Task<Results<
        Created<CreateBooleanProblemResponse>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromBody] CreateBooleanProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateBooleanProblemResult> result = await mediator.Send(new CreateBooleanProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateBooleanProblemResponse(result.Value.ProblemId));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class CreateBooleanProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public BooleanProblemInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateBooleanProblemResponse(Guid ProblemId);