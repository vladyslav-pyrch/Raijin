using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

public sealed record EdgeColoringInstance(Graph Graph, int ColourCount) : Instance
{
    public override string ProblemType() => ProblemTypes.EdgeColoringProblem;

    internal override SatEncoding ReduceToSat() => EdgeColoringToCspReduction.Apply(this)
        .CspInstance.ReduceToSat();

    internal override IReadOnlyDictionary<string, int> GetVariableMap()
    {
        EdgeColoringToCspReductionResult edgeColoringToCspResult = EdgeColoringToCspReduction.Apply(this);
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(edgeColoringToCspResult.CspInstance);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspToBoolResult.Instance);
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

        EdgeColoringToCspReductionResult edgeColoringToCspResult = EdgeColoringToCspReduction.Apply(this);
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(edgeColoringToCspResult.CspInstance);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspToBoolResult.Instance);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);

        Dictionary<int, EdgeColorAssignment> invertedSymbolTable = edgeColoringToCspResult.SymbolTable
            .ToDictionary(kvp => kvp.Key, kvp => dimacsResult.SymbolTable[tseitinResult.SymbolTable[cspToBoolResult.SymbolTable[kvp.Value]]])
            .Invert();

        List<EdgeColorAssignment> edgeColorAssignments = processedAssignments
            .Where(assignment => assignment.Value)
            .Where(assignment => invertedSymbolTable.ContainsKey(assignment.Index))
            .Select(assignment => invertedSymbolTable[assignment.Index])
            .ToList();

        return new EdgeColoringSolution(edgeColorAssignments);
    }
}