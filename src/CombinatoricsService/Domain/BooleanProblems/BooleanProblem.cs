using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public sealed record BooleanProblem(BoolExpr Root)
{
    public static string KindType => nameof(BooleanProblem);

    public SatEncoding ReduceToSat() => throw new NotImplementedException();

    public BooleanProblemSolution InterpretSolution(IReadOnlyList<int> assignment, VariableMap variableMap) =>
        throw new NotImplementedException();


    public BooleanProblem WithReplacementAt(NodePath path, BoolExpr replacement) =>
        this with { Root = Root.ReplaceAt(path.Steps, replacement) };

    public BooleanProblem WithReplacement(
        Func<BoolExpr, bool> predicate,
        Func<BoolExpr, BoolExpr> replacement
    ) => this with { Root = Root.Replace(predicate, replacement) };

    public BooleanProblem WithoutSubtreeAt(NodePath path) =>
        WithReplacementAt(path, new ConstExpr(true));
}