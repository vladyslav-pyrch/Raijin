using Raijin.CombinatoricsService.Domain.Graphs;

namespace Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

/// <summary>
///     An immutable Edge Colouring problem instance.
///     The goal is to assign one of <see cref="ColourCount" /> colours to each edge
///     such that no two edges sharing a vertex have the same colour.
/// </summary>
public sealed record EdgeColouringInstance(Graph Graph, int ColourCount)
{
    /// <summary>
    ///     Returns a new instance with the number of available colours changed.
    /// </summary>
    public EdgeColouringInstance WithColourCount(int colourCount) =>
        this with { ColourCount = colourCount };
}