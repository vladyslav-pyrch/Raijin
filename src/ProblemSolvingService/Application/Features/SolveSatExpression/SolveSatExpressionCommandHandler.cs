using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public sealed class SolveSatExpressionCommandHandler(ISatSolver solver) : IRequestHandler<SolveSatExpressionCommand, Result<SolveSatExpressionCommandResult>>
{
    private readonly SatExpressionParser _parser = new();

    public async Task<Result<SolveSatExpressionCommandResult>> Handle(SolveSatExpressionCommand request,
        CancellationToken cancellationToken = default)
    {
        Result<List<ClauseDto>> parsingResult = _parser.ParseClauses(request.SatExpression);
        Dictionary<int, string> variableNames = _parser.SymbolTable.Inverse();
        var satProblem = new SatProblem();

        if (parsingResult.IsFailed)
            return parsingResult.ToResult<SolveSatExpressionCommandResult>();
        foreach (Clause clause in parsingResult.Value.Select(clauseDto => clauseDto.ToClause()))
            satProblem.AddClause(clause);

        SatResult satResult = await solver.Solve(satProblem, cancellationToken);

        List<NamedSatVariableAssignmentDto> variableAssignments = satResult.Assignments
            .Select(va => new NamedSatVariableAssignmentDto(variableNames[va.SatVariable.Id], va.Assignment))
            .ToList();

        SolvingStatusDto solvingStatus = satResult.Status switch
        {
            SolvingStatus.Solvable => SolvingStatusDto.Satisfiable,
            SolvingStatus.Unsolvable => SolvingStatusDto.Unsatisfiable,
            SolvingStatus.Indeterminate => SolvingStatusDto.Indeterminate,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new SolveSatExpressionCommandResult(solvingStatus, variableAssignments);
    }
}
