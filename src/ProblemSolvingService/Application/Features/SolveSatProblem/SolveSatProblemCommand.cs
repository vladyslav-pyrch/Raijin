using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed record SolveSatProblemCommand(List<ClauseDto> Clauses) : IRequest<SolveSatProblemCommandResult>
{
    public SolveSatProblemInternalCommand ToInternalCommand()
    {
        var satProblem = new SatProblem();

        foreach (Clause clause in Clauses.Select(clauseDto => clauseDto.ToClause()))
            satProblem.AddClause(clause);

        return new SolveSatProblemInternalCommand(satProblem);
    }
}