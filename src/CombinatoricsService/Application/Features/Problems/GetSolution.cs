using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class GetSolutionHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetSolutionQuery, GetSolutionResult>
{
    public async Task<Result<GetSolutionResult>> Handle(
        GetSolutionQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus is SolvingStatus.Pending or SolvingStatus.Running)
            return new DomainError("Solving is not yet complete \u2014 solution is not available.");

        if (problem.SolvingStatus is SolvingStatus.Failed)
            return new DomainError("Solving failed \u2014 no solution is available.");

        if (problem.SolvingStatus is SolvingStatus.TimedOut)
            return new DomainError("Solving timed out \u2014 no solution is available.");

        if (problem.SolvingStatus is not SolvingStatus.Completed)
            return new DomainError($"Problem is in an unhandled status '{problem.SolvingStatus}'.");

        return new GetSolutionResult(problem.Solution, problem.Satisfiability);
    }
}

public sealed record GetSolutionQuery(
    Guid ProblemId
) : IRequest<GetSolutionResult>;

public sealed record GetSolutionResult(
    Solution? Solution,
    Satisfiability Satisfiability
);

public sealed class GetSolutionValidator : AbstractValidator<GetSolutionQuery>
{
    public GetSolutionValidator()
    {
        RuleFor(query => query.ProblemId).NotEmpty();
    }
}
