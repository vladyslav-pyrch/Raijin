using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class ListProblemsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems", Execute)
            .WithName("list problems")
            .WithTags("problems");
    }

    public static async Task<Results<
        Ok<ListProblemsResponse>,
        NotFound<ProblemDetails>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        Result<ListProblemsResult> result = await mediator.Send(new ListProblemsQuery(page, pageSize), cancellationToken);

        if (result.IsSuccess)
        {
            var value = result.Value;
            return TypedResults.Ok(new ListProblemsResponse(
                value.Items.Select(p => new ProblemSummaryResponse(
                    p.Id,
                    p.Name,
                    p.InstanceType,
                    p.SolvingStatus.ToString(),
                    p.Satisfiability.ToString(),
                    p.CreatedAt)).ToList(),
                value.Page,
                value.PageSize,
                value.TotalPages,
                value.TotalCount));
        }

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record ListProblemsResponse(
    IReadOnlyList<ProblemSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalPages,
    int TotalCount
);

public sealed record ProblemSummaryResponse(
    Guid Id,
    string Name,
    string? InstanceType,
    string SolvingStatus,
    string Satisfiability,
    DateTime CreatedAt
);
