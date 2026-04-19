using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public int GetVariableCount() =>
        Clauses.SelectMany(clause => clause.Literals)
            .DistinctBy(literal => literal.Variable.Name, StringComparer.Ordinal)
            .Count();

    public int GetClauseCount() => Clauses.Count;

    public override string ProblemType() => ProblemTypes.BooleanSatisfiabilityProblem;

    internal override SatEncoding ReduceToSat() => DimacsReduction.Apply(this).SatEncoding;

    internal override Solution InterpretSolution(IReadOnlyList<int> assignments)
    {
        var processedAssignments = assignments.Select(i => new
        {
            Index = Math.Abs(i),
            Value = i > 0
        }).ToArray();
        
        IReadOnlyDictionary<int, SatVariable> invertedSymbolTable = DimacsReduction.Apply(this)
            .SymbolTable.Invert();

        List<SatVariableAssignment> satVariableAssignments = processedAssignments.Select(
            assignment => new SatVariableAssignment(invertedSymbolTable[assignment.Index], assignment.Value)
        ).ToList();

        return new BooleanSatisfiabilitySolution(satVariableAssignments);
    }
}