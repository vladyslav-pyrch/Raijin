namespace Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

public sealed record EdgeColoringSolution(
    IReadOnlyList<EdgeColorAssignment> ColorAssignments
) : Solution;