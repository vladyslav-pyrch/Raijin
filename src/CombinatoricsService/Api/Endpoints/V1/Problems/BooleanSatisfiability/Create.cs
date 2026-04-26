using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class CreateSatProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/sat", Execute)
            .WithName("create sat problem")
            .WithTags("sat");
    }

    public static async Task<Results<
        Created<CreateSatProblemResponse>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromBody] CreateSatProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateSatProblemResult> result = await mediator.Send(new CreateSatProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateSatProblemResponse(result.Value.ProblemId));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        return TypedResults.InternalServerError();
    }
}

public sealed class CreateSatProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public SatInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateSatProblemResponse(Guid ProblemId);