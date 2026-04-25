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
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        return new GetProblemResult(
            problem.Id,
            problem.Name,
            problem.Description,
            problem.Solver,
            problem.Instance?.ProblemType(),
            problem.SolvingStatus,
            problem.Satisfiability,
            problem.CreatedAt,
            problem.UpdatedAt,
            problem.CompletedAt
        );
    }
}

public sealed record GetProblemResult(
    Guid Id,
    string Name,
    string Description,
    string? Solver,
    string? InstanceType,
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
