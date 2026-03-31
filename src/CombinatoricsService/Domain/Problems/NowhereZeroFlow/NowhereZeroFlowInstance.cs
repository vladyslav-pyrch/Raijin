using Raijin.CombinatoricsService.Domain.DirectedGraphs;

namespace Raijin.CombinatoricsService.Domain.Problems.NowhereZeroFlow;

/// <summary>
///     An immutable Nowhere-Zero Flow problem instance.
///     Flow values on each edge must be in {1, 2, …, <see cref="Modulus" />−1}
///     and must satisfy conservation constraints at every vertex.
/// </summary>
public sealed record NowhereZeroFlowInstance(
    DirectedGraph Graph,
    int Modulus,
    IReadOnlyDictionary<DirectedEdge, int> CapacityConstraints,
    IReadOnlyDictionary<DirectedVertex, int> Demands)
{
    // ── Modulus ──

    /// <summary>Returns a new instance with <paramref name="modulus" /> changed.</summary>
    public NowhereZeroFlowInstance WithModulus(int modulus) =>
        this with { Modulus = modulus };

    // ── Capacity constraints ──

    /// <summary>Returns a new instance with a capacity constraint on <paramref name="edge" />.</summary>
    public NowhereZeroFlowInstance WithCapacity(DirectedEdge edge, int capacity)
    {
        var updated = new Dictionary<DirectedEdge, int>(CapacityConstraints) { [edge] = capacity };
        return this with { CapacityConstraints = updated };
    }

    /// <summary>Returns a new instance with the capacity constraint on <paramref name="edge" /> removed.</summary>
    public NowhereZeroFlowInstance WithoutCapacity(DirectedEdge edge)
    {
        var updated = new Dictionary<DirectedEdge, int>(CapacityConstraints);
        updated.Remove(edge);
        return this with { CapacityConstraints = updated };
    }

    // ── Demand/supply per vertex ──

    /// <summary>Returns a new instance with a demand value at <paramref name="vertex" />.</summary>
    public NowhereZeroFlowInstance WithDemand(DirectedVertex vertex, int demand)
    {
        var updated = new Dictionary<DirectedVertex, int>(Demands) { [vertex] = demand };
        return this with { Demands = updated };
    }

    /// <summary>Returns a new instance with the demand at <paramref name="vertex" /> removed.</summary>
    public NowhereZeroFlowInstance WithoutDemand(DirectedVertex vertex)
    {
        var updated = new Dictionary<DirectedVertex, int>(Demands);
        updated.Remove(vertex);
        return this with { Demands = updated };
    }

    /// <summary>Creates a minimal instance with the given graph and modulus, and no constraints.</summary>
    public static NowhereZeroFlowInstance Create(DirectedGraph graph, int modulus) =>
        new(graph, modulus, new Dictionary<DirectedEdge, int>(), new Dictionary<DirectedVertex, int>());
}