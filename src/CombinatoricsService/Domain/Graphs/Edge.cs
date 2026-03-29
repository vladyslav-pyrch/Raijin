namespace Raijin.CombinatoricsService.Domain.Graphs;

/// <summary>
/// An undirected edge connecting two vertices.
/// </summary>
public sealed record Edge(Vertex U, Vertex V);
