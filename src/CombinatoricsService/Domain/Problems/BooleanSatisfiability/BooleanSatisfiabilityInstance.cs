using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public int GetVariableCount() => GetClauseCount() > 0 ? Clauses.Max(c => c.GetMaxVariableId()) : 0;

    public int GetClauseCount() => Clauses.Count;

    public override string ProblemType() => ProblemTypes.BooleanSatisfiabilityProblem;

    internal override (SatEncoding SatEncoding, VariableMap VariableMap) ReduceToSat()
    {
        IEnumerable<IEnumerable<int>> clauses = Clauses
            .Select(clause =>
                clause.Literals.Select(literal => literal.Variable.Id * (literal.Negated ? -1 : 1)).ToArray()
            ).ToArray();

        Dictionary<int, object> variableMap = Enumerable.Range(1, GetVariableCount())
            .ToDictionary(x => x, object (x) => new SatVariable(x));

        return (SatEncoding.Create(clauses), new VariableMap(variableMap));
    }

    internal override Solution InterpretSolution(IReadOnlyList<int> assignment)
    {
        VariableMap variableMap = ReduceToSat().VariableMap;

        List<SatVariableAssignment> satVariableAssignments = assignment.Select(i => new SatVariableAssignment(
                (SatVariable)variableMap.Entries[Math.Abs(i)],
                i > 0
            )
        ).ToList();

        return new BooleanSatisfiabilitySolution(satVariableAssignments);
    }
}