using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

public sealed record VertexColoringInstance(Graph Graph, int ColourCount) : Instance
{
    public override string ProblemType() => ProblemTypes.VertexColoringProblem;

    internal override SatEncoding ReduceToSat() =>
        VertexColoringToCspReduction.Apply(this).CspInstance.ReduceToSat();

    internal override IReadOnlyDictionary<string, int> GetVariableMap()
    {
        VertexColoringToCspReductionResult vertexToCspResult = VertexColoringToCspReduction.Apply(this);
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(vertexToCspResult.CspInstance);
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

        VertexColoringToCspReductionResult vertexToCspResult = VertexColoringToCspReduction.Apply(this);
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(vertexToCspResult.CspInstance);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspToBoolResult.Instance);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);

        Dictionary<int, VertexColorAssignment> invertedSymbolTable = vertexToCspResult.SymbolTable
            .ToDictionary(
                kvp => kvp.Key,
                kvp => dimacsResult.SymbolTable[tseitinResult.SymbolTable[cspToBoolResult.SymbolTable[kvp.Value]]]
            )
            .Invert();

        List<VertexColorAssignment> colorAssignments = processedAssignments
            .Where(a => a.Value)
            .Where(a => invertedSymbolTable.ContainsKey(a.Index))
            .Select(a => invertedSymbolTable[a.Index])
            .ToList();

        return new VertexColoringSolution(colorAssignments);
    }
}