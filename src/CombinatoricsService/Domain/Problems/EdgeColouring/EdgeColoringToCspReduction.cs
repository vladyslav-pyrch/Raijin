using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Graphs;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

internal static class EdgeColoringToCspReduction
{
    internal static EdgeColoringToCspReductionResult Apply(EdgeColoringInstance edgeColoringInstance)
    {
        ArgumentNullException.ThrowIfNull(edgeColoringInstance);

        IReadOnlyList<Edge> edges = edgeColoringInstance.Graph.Edges;
        int colourCount = edgeColoringInstance.ColourCount;
        List<string> colorStates = Enumerable.Range(1, colourCount).Select(c => c.ToString()).ToList();

        Dictionary<Vertex, List<Edge>> adjacency = [];
        foreach (Edge e in edges)
        {
            if (!adjacency.TryGetValue(e.U, out List<Edge>? uList))
                adjacency[e.U] = uList = [];
            if (!adjacency.TryGetValue(e.V, out List<Edge>? vList))
                adjacency[e.V] = vList = [];

            uList.Add(e);
            vList.Add(e);
        }

        List<DecisionVariable> variables = edges
            .Select(e => e.ToDecisionVariable(colorStates))
            .ToList();

        Dictionary<Edge, int> edgeIndex = edges.Select((e, i) => (e, i)).ToDictionary(t => t.e, t => t.i);
        HashSet<(int, int)> processedPairs = [];

        List<BoolExpr> constraints = [];
        foreach (Edge e1 in edges)
        {
            IEnumerable<Edge> adjacentEdges = (adjacency.TryGetValue(e1.U, out List<Edge>? uEdges) ? uEdges : [])
                .Concat(adjacency.TryGetValue(e1.V, out List<Edge>? vEdges) ? vEdges : [])
                .Where(e => e != e1);

            foreach (Edge e2 in adjacentEdges)
            {
                int i1 = edgeIndex[e1];
                int i2 = edgeIndex[e2];
                (int lo, int hi) = i1 < i2 ? (i1, i2) : (i2, i1);
                if (!processedPairs.Add((lo, hi))) continue;

                constraints.AddRange(
                    from c in Enumerable.Range(1, colourCount)
                    let v1 = e1.ToEdgeColorAssignment(c).ToDecisionVariableStateAssignment().ToBoolVar()
                    let v2 = e2.ToEdgeColorAssignment(c).ToDecisionVariableStateAssignment().ToBoolVar()
                    select v1.And(v2).Negated()
                );
            }
        }

        var csp = new CspInstance(variables, constraints);

        Dictionary<EdgeColorAssignment, DecisionVariableStateAssignment> symbolTable = edges
            .SelectMany(e => Enumerable.Range(1, colourCount).Select(e.ToEdgeColorAssignment)
            ).ToDictionary(eca => eca, eca => eca.ToDecisionVariableStateAssignment());

        return new EdgeColoringToCspReductionResult(csp, symbolTable);
    }
}

internal static class InternalExtensions
{
    public static DecisionVariable ToDecisionVariable(this Edge edge, List<string> colorStates) =>
        new(edge.Label, colorStates);

    public static EdgeColorAssignment ToEdgeColorAssignment(this Edge edge, int colorState) =>
        new(edge.Label, colorState);

    public static DecisionVariableStateAssignment ToDecisionVariableStateAssignment(this EdgeColorAssignment edgeColorAssignment) =>
        new(edgeColorAssignment.EdgeLabel, edgeColorAssignment.Color.ToString());
}