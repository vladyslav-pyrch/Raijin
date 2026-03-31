namespace Raijin.CombinatoricsService.Domain.DirectedGraphs;

/// <summary>
/// An immutable directed graph represented as a set of vertices and directed edges.
/// </summary>
public sealed record DirectedGraph(IReadOnlyList<DirectedVertex> Vertices, IReadOnlyList<DirectedEdge> Edges)
{
    /// <summary>Returns a new graph with <paramref name="vertex"/> appended.</summary>
    public DirectedGraph WithVertex(DirectedVertex vertex) =>
        this with { Vertices = [.. Vertices, vertex] };

    /// <summary>Returns a new graph with <paramref name="vertex"/> and all its incident edges removed.</summary>
    public DirectedGraph WithoutVertex(DirectedVertex vertex) =>
        this with
        {
            Vertices = Vertices.Where(v => v != vertex).ToList(),
            Edges = Edges.Where(e => e.From != vertex && e.To != vertex).ToList()
        };

    /// <summary>Returns a new graph with <paramref name="edge"/> appended.</summary>
    public DirectedGraph WithEdge(DirectedEdge edge) =>
        this with { Edges = [.. Edges, edge] };

    /// <summary>Returns a new graph with the edge from <paramref name="from"/> to <paramref name="to"/> removed.</summary>
    public DirectedGraph WithoutEdge(DirectedVertex from, DirectedVertex to) =>
        this with
        {
            Edges = Edges.Where(e => !(e.From == from && e.To == to)).ToList()
        };

    /// <summary>Creates an empty directed graph.</summary>
    public static DirectedGraph Empty => new([], []);
}
