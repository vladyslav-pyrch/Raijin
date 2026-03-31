using Raijin.CombinatoricsService.Domain.Graphs;

namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

/// <summary>
///     An immutable Vertex Colouring problem instance.
///     The goal is to assign one of <see cref="ColourCount" /> colours to each vertex
///     such that no two adjacent vertices share the same colour.
/// </summary>
public sealed record VertexColouringInstance(Graph Graph, int ColourCount)
{
    /// <summary>
    ///     Returns a new instance with the number of available colours changed.
    /// </summary>
    public VertexColouringInstance WithColourCount(int colourCount) =>
        this with { ColourCount = colourCount };
}