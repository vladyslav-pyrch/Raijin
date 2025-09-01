using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

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