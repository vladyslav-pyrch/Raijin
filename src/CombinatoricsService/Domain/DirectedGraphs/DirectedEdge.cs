namespace Raijin.CombinatoricsService.Domain.DirectedGraphs;

/// <summary>
/// A directed edge from <see cref="From"/> to <see cref="To"/>.
/// </summary>
public sealed record DirectedEdge(DirectedVertex From, DirectedVertex To);
