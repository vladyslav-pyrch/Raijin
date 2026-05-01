using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class ListProblemsHandler(
    IProblemRepository problemRepository
) : IRequestHandler<ListProblemsQuery, ListProblemsResult>
{
    public async Task<Result<ListProblemsResult>> Handle(
        ListProblemsQuery request,
        CancellationToken cancellationToken
    )
    {
        ListProblemsResult result = await problemRepository.ListProblems(request.Page, request.PageSize, cancellationToken);

        if (result.TotalCount > 0 && request.Page > result.TotalPages)
            return new NotFoundError($"Page {request.Page} does not exist. Total pages: {result.TotalPages}.");

        return result;
    }
}

public sealed record ListProblemsQuery(int Page, int PageSize) : IRequest<ListProblemsResult>;

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
    string InstanceType,
    SolvingStatus SolvingStatus,
    Satisfiability Satisfiability,
    DateTime CreatedAt
);

public sealed class ListProblemsValidator : AbstractValidator<ListProblemsQuery>
{
    public ListProblemsValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}