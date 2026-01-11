using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public sealed class SolveSatExpressionHandler(ISatSolver solver) : IRequestHandler<SolveSatExpressionCommand, Result<SolveSatExpressionResult>>
{
    private readonly SatExpressionParser _parser = new();

    public async Task<Result<SolveSatExpressionResult>> Handle(SolveSatExpressionCommand request,
        CancellationToken cancellationToken = default)
    {
        Result<List<ClauseDto>> parsingResult = _parser.ParseClauses(request.SatExpression);
        Dictionary<int, string> variableNames = _parser.SymbolTable.Inverse();
        var satProblem = new SatProblem();

        if (parsingResult.IsFailed)
            return parsingResult.ToResult<SolveSatExpressionResult>();
        foreach (Clause clause in parsingResult.Value.Select(clauseDto => clauseDto.ToClause()))
            satProblem.AddClause(clause);

        SatResult satResult = await solver.Solve(satProblem, cancellationToken);

        List<NamedSatVariableAssignmentDto> variableAssignments = satResult.Assignments
            .Select(va => new NamedSatVariableAssignmentDto(variableNames[va.SatVariable.Id], va.Assignment))
            .ToList();

        SolvingStatusDto solvingStatus = satResult.Status.ToDto();

        return new SolveSatExpressionResult(solvingStatus, variableAssignments);
    }
}
