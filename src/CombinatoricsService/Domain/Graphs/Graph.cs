namespace Raijin.CombinatoricsService.Domain.Graphs;

/// <summary>
/// An immutable undirected graph represented as a set of vertices and edges.
/// </summary>
public sealed record Graph(IReadOnlyList<Vertex> Vertices, IReadOnlyList<Edge> Edges)
{
    /// <summary>
    /// Returns a new graph with <paramref name="vertex"/> appended.
    /// </summary>
    public Graph WithVertex(Vertex vertex) =>
        this with { Vertices = [.. Vertices, vertex] };

    /// <summary>
    /// Returns a new graph with <paramref name="vertex"/> and all its incident edges removed.
    /// </summary>
    public Graph WithoutVertex(Vertex vertex) =>
        this with
        {
            Vertices = Vertices.Where(v => v != vertex).ToList(),
            Edges = Edges.Where(e => e.U != vertex && e.V != vertex).ToList()
        };

    /// <summary>
    /// Returns a new graph with <paramref name="edge"/> appended.
    /// </summary>
    public Graph WithEdge(Edge edge) =>
        this with { Edges = [.. Edges, edge] };

    /// <summary>
    /// Returns a new graph with the edge between <paramref name="u"/> and <paramref name="v"/> removed.
    /// </summary>
    public Graph WithoutEdge(Vertex u, Vertex v) =>
        this with
        {
            Edges = Edges.Where(e => !(e.U == u && e.V == v) && !(e.U == v && e.V == u)).ToList()
        };

    /// <summary>
    /// Creates an empty graph with no vertices or edges.
    /// </summary>
    public static Graph Empty => new([], []);
}
