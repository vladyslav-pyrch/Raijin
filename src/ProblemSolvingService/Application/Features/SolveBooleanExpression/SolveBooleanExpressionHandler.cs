using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public sealed class SolveBooleanExpressionHandler(ISatSolver solver) : IRequestHandler<SolveBooleanExpressionCommand, Result<SolveBooleanExpressionResult>>
{
    public async Task<Result<SolveBooleanExpressionResult>> Handle(SolveBooleanExpressionCommand request, CancellationToken cancellationToken = default)
    {
        Result<IBooleanExpression> expressionResult = BooleanExpressionParser.ParseExpression(request.Expression);

        if (expressionResult.IsFailed)
            return expressionResult.ToResult<SolveBooleanExpressionResult>();

        IBooleanExpression expression = expressionResult.Value;

        BooleanFormula formula = new(expression);
        SatProblem satProblem = new();
        Dictionary<SatVariable, Variable> variableMap = formula.TransformToSat(satProblem).Inverse();

        SatResult satResult = await solver.Solve(satProblem, cancellationToken);

        SolvingStatusDto solvingStatus = satResult.Status.ToDto();
        List<VariableAssignmentDto> variableAssignments = satResult.Assignments
            .Where(satVariableAssignment => variableMap.ContainsKey(satVariableAssignment.SatVariable))
            .Select(satVariableAssignment =>
                new VariableAssignmentDto(variableMap[satVariableAssignment.SatVariable].Name,
                    satVariableAssignment.Assignment)).ToList();

        return new SolveBooleanExpressionResult(solvingStatus, variableAssignments);
    }
}