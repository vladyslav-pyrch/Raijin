using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilityInstance(IReadOnlyList<Clause> Clauses) : Instance
{
    public override string ProblemType() => ProblemTypes.BooleanSatisfiabilityProblem;

    internal override SatEncoding ReduceToSat() => DimacsReduction.Apply(this).SatEncoding;

    internal override IReadOnlyDictionary<string, int> GetVariableMap() =>
        DimacsReduction.Apply(this).SymbolTable
            .ToDictionary(kvp => kvp.Key.Name, kvp => kvp.Value);

    internal override Solution InterpretSolution(IReadOnlyList<int> assignments)
    {
        var processedAssignments = assignments.Select(i => new
        {
            Index = Math.Abs(i),
            Value = i > 0
        }).ToArray();

        IReadOnlyDictionary<int, SatVariable> invertedSymbolTable = DimacsReduction.Apply(this)
            .SymbolTable.Invert();

        List<SatVariableAssignment> satVariableAssignments = processedAssignments
            .Select(assignment => new SatVariableAssignment(invertedSymbolTable[assignment.Index], assignment.Value)
            ).ToList();

        return new BooleanSatisfiabilitySolution(satVariableAssignments);
    }
}