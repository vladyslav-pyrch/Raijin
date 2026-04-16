using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

public sealed record BooleanProblemInstance(BoolExpr Root) : Instance
{
    public override string ProblemType() => ProblemTypes.BooleanProblem;

    internal override (SatEncoding SatEncoding, VariableMap VariableMap) ReduceToSat()
    {
        (BooleanSatisfiabilityInstance bsiInstance, _) = TseitinTransform.Apply(Root);
        return bsiInstance.ReduceToSat();
    }

    internal override Solution InterpretSolution(IReadOnlyList<int> assignment)
    {
        (BooleanSatisfiabilityInstance bsiInstance, IReadOnlyDictionary<BoolVar, SatVariable> boolVarToSatVar) = TseitinTransform.Apply(Root);
        (_, VariableMap variableMap) = bsiInstance.ReduceToSat();

        var nameToBoolVar = boolVarToSatVar
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        var resultAssignment = new List<BooleanVariableAssignment>();

        foreach (int literal in assignment)
        {
            int index = Math.Abs(literal);
            if (!variableMap.Entries.TryGetValue(index, out object? obj))
                continue;
            if (obj is not SatVariable satVar)
                continue;
            if (!nameToBoolVar.TryGetValue(satVar, out BoolVar? boolVar))
                continue; // auxiliary variable — skip

            resultAssignment.Add(new BooleanVariableAssignment(boolVar, literal > 0));
        }

        return new BooleanProblemSolution(resultAssignment);
    }

    public BooleanProblemInstance WithReplacementAt(NodePath path, BoolExpr replacement) =>
        this with { Root = Root.ReplaceAt(path.Steps, replacement) };

    public BooleanProblemInstance WithReplacement(
        Func<BoolExpr, bool> predicate,
        Func<BoolExpr, BoolExpr> replacement
    ) => this with { Root = Root.Replace(predicate, replacement) };

    public BooleanProblemInstance WithoutSubtreeAt(NodePath path) =>
        WithReplacementAt(path, new ConstExpr(true));
}
