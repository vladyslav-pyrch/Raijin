using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public int GetVariableCount() => GetClauseCount() > 0 ? Clauses.Max(c => c.GetMaxVariableId()) : 0;

    public int GetClauseCount() => Clauses.Count;

    public override string ProblemType() => ProblemTypes.BooleanSatisfiabilityProblem;

    internal override SatEncoding ReduceToSat()
    {
        IEnumerable<IEnumerable<int>> clauses = Clauses
            .Select(clause =>
                clause.Literals.Select(literal => literal.Variable.Id * (literal.Negated ? -1 : 1)).ToArray()
            ).ToArray();

        var variableMap = Enumerable.Range(1, GetVariableCount())
            .ToDictionary(x => x, x => x.ToString());

        return SatEncoding.Create(clauses, new VariableMap(variableMap));
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