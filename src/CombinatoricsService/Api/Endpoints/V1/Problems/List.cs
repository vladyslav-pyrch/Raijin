using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class ListProblemsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems", Execute)
            .WithName("list problems")
            .WithTags("problems")
            .Produces<ListProblemsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> Execute(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        Result<ListProblemsResult> result = await mediator.Send(new ListProblemsQuery(page, pageSize), cancellationToken);

        if (result.IsSuccess)
        {
            ListProblemsResult? value = result.Value;
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

        return result.ToProblemResult();
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