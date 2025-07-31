using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

public record SolveSatProblemRequest(List<ClauseRequest> Clauses)
{
    public SolveSatProblemCommand ToSolveStaProblemCommand() => new(Clauses.Select(c => c.ToClauseDto()).ToList());
}