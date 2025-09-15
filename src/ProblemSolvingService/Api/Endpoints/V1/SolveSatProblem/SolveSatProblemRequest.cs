using Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Requests;
using Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatProblem;

public sealed record SolveSatProblemRequest(List<ClauseRequest> Clauses)
{
    public SolveSatProblemCommand ToSolveStaProblemCommand() => new(Clauses.Select(c => c.ToClauseDto()).ToList());
}