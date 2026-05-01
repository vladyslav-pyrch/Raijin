using Raijin.CombinatoricsService.Domain.Graphs;

namespace Raijin.CombinatoricsService.Domain.Problems.IndependentSet;

/// <summary>
///     An immutable Independent Set problem instance.
///     The goal is to find a subset of exactly <see cref="Size" /> vertices in <see cref="Graph" />
///     such that no two vertices in the subset are adjacent.
/// </summary>
public sealed record IndependentSetInstance(Graph Graph, int Size)
{
    /// <summary>
    ///     Returns a new instance with the target set size changed.
    /// </summary>
    public IndependentSetInstance WithSize(int size) =>
        this with
        {
            Size = size
        };
}