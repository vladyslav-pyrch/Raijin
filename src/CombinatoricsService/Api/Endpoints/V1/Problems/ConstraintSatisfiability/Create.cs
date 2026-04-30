using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class CreateCspProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/csp", Execute)
            .WithName("create csp problem")
            .WithTags("csp")
            .Produces<CreateCspProblemResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> Execute(
        [FromBody] CreateCspProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<CreateCspProblemResult> result = await mediator.Send(new CreateCspProblemCommand(
            new ProblemDetailsDto(
                request.Name,
                request.Description ?? string.Empty
            ),
            request.Instance
        ), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateCspProblemResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }
}

public sealed class CreateCspProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public CspInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateCspProblemResponse(Guid ProblemId);
