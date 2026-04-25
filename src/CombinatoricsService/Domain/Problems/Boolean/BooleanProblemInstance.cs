using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

public sealed record BooleanProblemInstance(BoolExpr Root) : Instance
{
    public override string ProblemType() => ProblemTypes.BooleanProblem;

    public BooleanProblemInstance WithReplacementAt(NodePath path, BoolExpr replacement) =>
        this with { Root = Root.ReplaceAt(path.Steps, replacement) };

    public BooleanProblemInstance WithReplacement(
        Func<BoolExpr, bool> predicate,
        Func<BoolExpr, BoolExpr> replacement
    ) => this with { Root = Root.Replace(predicate, replacement) };

    public BooleanProblemInstance WithoutSubtreeAt(NodePath path) =>
        WithReplacementAt(path, new ConstExpr(true));

    internal override SatEncoding ReduceToSat() => TseitinTransform.Apply(this).Instance.ReduceToSat();

    internal override IReadOnlyDictionary<string, int> GetVariableMap()
    {
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(this);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);
        return dimacsResult.SymbolTable.ToDictionary(kvp => kvp.Key.Name, kvp => kvp.Value);
    }

    internal override Solution InterpretSolution(IReadOnlyList<int> assignments)
    {
        var processedAssignments = assignments.Select(i => new
        {
            Index = Math.Abs(i),
            Value = i > 0
        }).ToArray();

        TseitinTransformResult tseitinResult = TseitinTransform.Apply(this);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);

        Dictionary<int, BoolVar> invertedSymbolTable = tseitinResult.SymbolTable
            .ToDictionary(kvp => kvp.Key, kvp => dimacsResult.SymbolTable[kvp.Value])
            .Invert();

        List<BooleanVariableAssignment> resultAssignment = processedAssignments
            .Where(assignment => invertedSymbolTable.ContainsKey(assignment.Index))
            .Select(assignment => new BooleanVariableAssignment(invertedSymbolTable[assignment.Index], assignment.Value))
            .ToList();

        return new BooleanProblemSolution(resultAssignment);
    }
}