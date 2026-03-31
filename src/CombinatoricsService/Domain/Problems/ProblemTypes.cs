namespace Raijin.CombinatoricsService.Domain.Problems;

public static class ProblemTypes
{
    public const string BooleanProblem = "bool";

    public const string BooleanSatisfiabilityProblem = "sat";

    public const string ConstraintSatisfiabilityProblem = "csp";

    public const string VertexColoringProblem = "vertex-coloring";

    public const string EdgeColoringProblem = "edge-coloring";

    public const string IndependentSetProblem = "independent-set";

    public const string NowhereZeroFlowProblem = "nowhere-zero-flow";

    public const string SchedulingProblem = "scheduling";

    public static bool IsValid(string problemType) =>
        problemType == BooleanProblem ||
        problemType == BooleanSatisfiabilityProblem ||
        problemType == ConstraintSatisfiabilityProblem ||
        problemType == VertexColoringProblem ||
        problemType == EdgeColoringProblem ||
        problemType == IndependentSetProblem ||
        problemType == NowhereZeroFlowProblem ||
        problemType == SchedulingProblem;

    public static IEnumerable<string> GetAll() =>
    [
        BooleanProblem,
        BooleanSatisfiabilityProblem,
        ConstraintSatisfiabilityProblem,
        VertexColoringProblem,
        EdgeColoringProblem,
        IndependentSetProblem,
        NowhereZeroFlowProblem,
        SchedulingProblem
    ];
}