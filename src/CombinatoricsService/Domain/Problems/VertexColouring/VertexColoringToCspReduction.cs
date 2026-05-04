using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

internal static class VertexColoringToCspReduction
{
    internal static VertexColoringToCspReductionResult Apply(VertexColoringInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        IReadOnlyList<Vertex> vertices = instance.Graph.Vertices;
        IReadOnlyList<Edge> edges = instance.Graph.Edges;
        int colourCount = instance.ColourCount;
        List<string> colourStates = Enumerable.Range(1, colourCount).Select(c => c.ToString()).ToList();

        // One decision variable per vertex
        List<DecisionVariable> variables = vertices
            .Select(v => v.ToDecisionVariable(colourStates))
            .ToList();

        // For each edge (u, v), for each colour c: NOT (u==c AND v==c)
        List<BoolExpr> constraints = [];
        foreach (Edge edge in edges)
        {
            constraints.AddRange(
                from c in Enumerable.Range(1, colourCount)
                let v1 = edge.U.ToVertexColorAssignment(c).ToDecisionVariableStateAssignment().ToBoolVar()
                let v2 = edge.V.ToVertexColorAssignment(c).ToDecisionVariableStateAssignment().ToBoolVar()
                select v1.And(v2).Negated()
            );
        }

        var csp = new CspInstance(variables, constraints);

        Dictionary<VertexColorAssignment, DecisionVariableStateAssignment> symbolTable = vertices
            .SelectMany(v => Enumerable.Range(1, colourCount).Select(v.ToVertexColorAssignment))
            .ToDictionary(vca => vca, vca => vca.ToDecisionVariableStateAssignment());

        return new VertexColoringToCspReductionResult(csp, symbolTable);
    }
}

internal static class InternalExtensions
{
    public static DecisionVariable ToDecisionVariable(this Vertex vertex, List<string> colourStates) =>
        new(vertex.Id, colourStates);

    public static VertexColorAssignment ToVertexColorAssignment(this Vertex vertex, int colour) =>
        new(vertex.Id, colour);

    public static DecisionVariableStateAssignment ToDecisionVariableStateAssignment(this VertexColorAssignment assignment) =>
        new(assignment.VertexId, assignment.Color.ToString());
}