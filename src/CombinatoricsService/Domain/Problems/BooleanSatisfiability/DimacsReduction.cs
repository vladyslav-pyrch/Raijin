namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

internal class DimacsReduction
{
    public static DimacsReductionResult Apply(BooleanSatisfiabilityInstance instance)
    {
        List<SatVariable> variables = instance.Clauses.SelectMany(clause => clause.Literals)
            .Select(literal => literal.Variable)
            .DistinctBy(variable => variable.Name, StringComparer.Ordinal)
            .ToList();

        Dictionary<SatVariable, int> variableToIndex = variables
            .Select((variable, index) => (variable, index: index + 1))
            .ToDictionary(x => x.variable, x => x.index);

        IEnumerable<IEnumerable<int>> dimacs = instance.Clauses
            .Select(clause => clause.Literals
                .Select(lit =>
                {
                    int idx = variableToIndex[lit.Variable];
                    return lit.Negated ? -idx : idx;
                }).ToArray()
            ).ToArray();

        return new DimacsReductionResult(SatEncoding.Create(dimacs), variableToIndex);
    }
}