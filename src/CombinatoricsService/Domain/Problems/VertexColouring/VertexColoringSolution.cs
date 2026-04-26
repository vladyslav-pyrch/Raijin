namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

public sealed record VertexColoringSolution(
    IReadOnlyList<VertexColorAssignment> ColorAssignments
) : Solution;

