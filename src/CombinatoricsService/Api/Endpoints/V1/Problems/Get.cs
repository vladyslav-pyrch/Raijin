using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}", Execute)
            .WithName("get problem")
            .WithTags("problems");
    }

    public static async Task<Results<
            Ok<GetProblemResponse>,
            NotFound<ProblemDetails>,
            ValidationProblem,
            InternalServerError>>
        Execute(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken)
    {
        Result<GetProblemResult> result = await mediator.Send(new GetProblemQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetProblemResponse(
                result.Value.Id,
                result.Value.Name,
                result.Value.Description,
                result.Value.Solver,
                result.Value.InstanceType,
                result.Value.SolvingStatus.ToString(),
                result.Value.Satisfiability.ToString(),
                result.Value.CreatedAt,
                result.Value.UpdatedAt,
                result.Value.CompletedAt
            ));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetProblemResponse(
    Guid Id,
    string Name,
    string Description,
    string? Solver,
    string InstanceType,
    string SolvingStatus,
    string Satisfiability,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
);