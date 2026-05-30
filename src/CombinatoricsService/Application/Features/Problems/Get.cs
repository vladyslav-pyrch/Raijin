using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record GetProblemQuery(Guid ProblemId) : IRequest<GetProblemResult>;

public sealed class GetProblemHandler(
    IProblemRepository problemRepository,
    ILogger<GetProblemHandler> logger
) : IRequestHandler<GetProblemQuery, GetProblemResult>
{
    public async Task<Result<GetProblemResult>> Handle(
        GetProblemQuery request,
        CancellationToken cancellationToken)
    {
        GetProblemResult? problem = await problemRepository.GetSummaryById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        logger.LogDebug(
            "Problem summary read. ProblemId={ProblemId} SolvingStatus={SolvingStatus} Satisfiability={Satisfiability}",
            problem.Id,
            problem.SolvingStatus,
            problem.Satisfiability);

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
    DateTime? StartedSolvingAt,
    DateTime? CompletedAt,
    TimeSpan? ElapsedTime
);

public sealed class GetProblemValidator : AbstractValidator<GetProblemQuery>
{
    public GetProblemValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
