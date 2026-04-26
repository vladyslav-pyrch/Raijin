using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record GetProblemQuery(Guid ProblemId) : IRequest<GetProblemResult>;

public sealed class GetProblemHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetProblemQuery, GetProblemResult>
{
    public async Task<Result<GetProblemResult>> Handle(
        GetProblemQuery request,
        CancellationToken cancellationToken)
    {
        GetProblemResult? problem = await problemRepository.GetSummaryById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        return problem;
    }
}

public sealed record GetProblemResult(
    Guid Id,
    string Name,
    string Description,
    string? Solver,
    string InstanceType,
    SolvingStatus SolvingStatus,
    Satisfiability Satisfiability,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
);

public sealed class GetProblemValidator : AbstractValidator<GetProblemQuery>
{
    public GetProblemValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
