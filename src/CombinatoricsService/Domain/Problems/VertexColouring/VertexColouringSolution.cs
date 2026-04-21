namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

public sealed record VertexColouringSolution(
    IReadOnlyList<VertexColorAssignment> ColorAssignments
) : Solution;

