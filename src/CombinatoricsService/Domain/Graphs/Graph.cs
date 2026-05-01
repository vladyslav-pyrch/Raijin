namespace Raijin.CombinatoricsService.Domain.Graphs;

public sealed record Graph(IReadOnlyList<Vertex> Vertices, IReadOnlyList<Edge> Edges);