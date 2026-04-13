namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public int GetVariableCount() => Clauses.SelectMany(clause => clause.Literals)
        .DistinctBy(literal => literal.Variable.Name, StringComparer.Ordinal)
        .Count();

    public int GetClauseCount() => Clauses.Count;

    public override string ProblemType() => ProblemTypes.BooleanSatisfiabilityProblem;

    internal override (SatEncoding SatEncoding, VariableMap VariableMap) ReduceToSat()
    {
        var sortedVariables = Clauses.SelectMany(clause => clause.Literals)
            .Select(literal => literal.Variable)
            .DistinctBy(variable => variable.Name, StringComparer.Ordinal)
            .OrderBy(variable => variable.Name, StringComparer.Ordinal)
            .ToList();
        
        var variableToIndex = sortedVariables
            .Select((variable, index) => (variable, index: index + 1))
            .ToDictionary(x => x.variable, x => x.index);

        var variableMap = sortedVariables.ToDictionary<SatVariable, int, object>(
            variable => variableToIndex[variable],
            variable => variable
        );

        IEnumerable<IEnumerable<int>> dimacs = Clauses
            .Select(clause => clause.Literals
                .Select(lit =>
                {
                    int idx = variableToIndex[lit.Variable];
                    return lit.Negated ? -idx : idx;
                })
                .ToArray())
            .ToArray();

        return (SatEncoding.Create(dimacs), new VariableMap(variableMap));
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
