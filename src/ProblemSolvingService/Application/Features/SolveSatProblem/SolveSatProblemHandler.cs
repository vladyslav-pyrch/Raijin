using FluentValidation;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemHandler(ISatSolver satSolver)
    : IRequestHandler<SolveSatProblemCommand, SolveSatProblemResult>
{
    public async Task<SolveSatProblemResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
    {
        await new SolveSatProblemValidator().ValidateAndThrowAsync(command, cancellationToken);

        var satProblem = new SatProblem();

        foreach (Clause clause in command.Clauses.Select(clauseDto => clauseDto.ToClause()))
            satProblem.AddClause(clause);

        SatResult satResult = await satSolver.Solve(satProblem, cancellationToken);

        return SolveSatProblemResult.FromSatResult(satResult);
    }
}