using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
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
            .WithTags("bool")
            .Produces<CreateBooleanProblemResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> Execute(
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

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateBooleanProblemResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }
}

public sealed class CreateBooleanProblemRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public BooleanProblemInstanceDto Instance { get; set; } = null!;
}

public sealed record CreateBooleanProblemResponse(Guid ProblemId);
