using FluentValidation;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemCommandHandler(ISatSolver satSolver)
    : IRequestHandler<SolveSatProblemCommand, SolveSatProblemCommandResult>
{
    public async Task<SolveSatProblemCommandResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
    {
        await new SolveSatProblemCommandValidator().ValidateAndThrowAsync(command, cancellationToken);

        var satProblem = new SatProblem();

        foreach (Clause clause in command.Clauses.Select(clauseDto => clauseDto.ToClause()))
            satProblem.AddClause(clause);

        SatResult satResult = await satSolver.Solve(satProblem, cancellationToken);

        return SolveSatProblemCommandResult.FromSatResult(satResult);
    }
}