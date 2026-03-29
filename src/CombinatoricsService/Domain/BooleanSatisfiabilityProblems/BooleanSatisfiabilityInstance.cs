using System.Text;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public override string ProblemKind => "sat";

    public int GetVariableCount() => Clauses.Max(c => c.GetMaxVariableId());

    public int GetClauseCount() => Clauses.Count;

    public string ToDimacsFormat()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"p cnf {GetVariableCount()} {GetClauseCount()}");
        foreach (Clause clause in Clauses)
            sb.AppendLine(clause.ToDimacsFormat());
        return sb.ToString();
    }

    internal override SatEncoding ReduceToSat()
    {
        string dimacs = ToDimacsFormat();

        var variableMap = Enumerable.Range(1, GetVariableCount())
            .ToDictionary(x => x, x => x.ToString());

        return new SatEncoding(dimacs, new VariableMap(variableMap));
    }

    internal override Solution InterpretSolution(IReadOnlyList<int> assignment, VariableMap variableMap)
    {
        var assignments = assignment.Select(i => new SatVariableAssignment(
                new SatVariable(int.Parse(variableMap.Entries[Math.Abs(i)])),
                i > 0
            )
        ).ToList();

        return new BooleanSatisfiabilitySolution(assignments);
    }
}