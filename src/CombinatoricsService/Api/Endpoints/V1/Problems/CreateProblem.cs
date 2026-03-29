using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public class CreateProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/problems", Execute)
            .WithName("create problem")
            .WithTags("problems");
    }

    public static async Task<Results<Created<CreateProblemResponse>, ValidationProblem, InternalServerError>> Execute(
        [FromBody] CreateProblemRequest request,
        [FromServices] IMediator mediator
    )
    {
        Result<CreateProblemResult> result = await mediator.Send(new CreateProblemCommand(
            request.Name,
            request.Description,
            request.ProblemKind
        ), CancellationToken.None);

        if (result.IsValidationError())
            return TypedResults.ValidationProblem(result.ToValidationErrorDictionary());

        if (result.IsFailed)
            return TypedResults.InternalServerError();

        return TypedResults.Created("problems/{ProblemId}", new CreateProblemResponse
        {
            ProblemId = result.Value.Id
        });
    }
}

public class CreateProblemRequest
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ProblemKind { get; set; } = null!;
}

public class CreateProblemResponse
{
    public Guid ProblemId { get; set; }
}