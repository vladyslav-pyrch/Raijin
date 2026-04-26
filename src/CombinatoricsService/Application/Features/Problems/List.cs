using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record ListProblemsQuery(int Page, int PageSize) : IRequest<ListProblemsResult>;

public sealed class ListProblemsHandler(
    IProblemRepository problemRepository
) : IRequestHandler<ListProblemsQuery, ListProblemsResult>
{
    public async Task<Result<ListProblemsResult>> Handle(
        ListProblemsQuery request,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Problem> items, int totalCount) =
            await problemRepository.GetPage(request.Page, request.PageSize, cancellationToken);

        int totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        if (totalCount > 0 && request.Page > totalPages)
            return new NotFoundError($"Page {request.Page} does not exist. Total pages: {totalPages}.");

        var summaries = items
            .Select(p => new ProblemSummary(
                p.Id,
                p.Name,
                p.Instance.ProblemType(),
                p.SolvingStatus,
                p.Satisfiability,
                p.CreatedAt))
            .ToList();

        return new ListProblemsResult(summaries, request.Page, request.PageSize, totalPages, totalCount);
    }
}

public sealed record ListProblemsResult(
    IReadOnlyList<ProblemSummary> Items,
    int Page,
    int PageSize,
    int TotalPages,
    int TotalCount
);

public sealed record ProblemSummary(
    Guid Id,
    string Name,
    string? InstanceType,
    SolvingStatus SolvingStatus,
    Satisfiability Satisfiability,
    DateTime CreatedAt
);

public sealed class ListProblemsValidator : AbstractValidator<ListProblemsQuery>
{
    public ListProblemsValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
