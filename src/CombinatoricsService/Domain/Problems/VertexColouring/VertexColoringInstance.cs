using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

/// <summary>
///     An immutable Vertex Colouring problem instance.
///     The goal is to assign one of <see cref="ColourCount" /> colours to each vertex
///     such that no two adjacent vertices share the same colour.
/// </summary>
public sealed record VertexColoringInstance(Graph Graph, int ColourCount) : Instance
{
    /// <summary>
    ///     Returns a new instance with the number of available colours changed.
    /// </summary>
    public VertexColoringInstance WithColourCount(int colourCount) =>
        this with { ColourCount = colourCount };

    public override string ProblemType() => ProblemTypes.VertexColoringProblem;

    internal override SatEncoding ReduceToSat() =>
        VertexColouringToCspReduction.Apply(this).CspInstance.ReduceToSat();

    internal override IReadOnlyDictionary<string, int> GetVariableMap()
    {
        VertexColouringToCspReductionResult vertexResult = VertexColouringToCspReduction.Apply(this);
        CspToBooleanReductionResult cspResult = CspToBooleanReduction.Apply(vertexResult.CspInstance);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspResult.Instance);
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

        VertexColouringToCspReductionResult vertexToCspResult = VertexColouringToCspReduction.Apply(this);
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

        return new VertexColouringSolution(colorAssignments);
    }
}