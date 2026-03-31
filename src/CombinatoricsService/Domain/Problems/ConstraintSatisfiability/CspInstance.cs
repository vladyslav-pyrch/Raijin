using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

/// <summary>
///     An immutable Constraint Satisfaction Problem instance.
///     Variables define the search space; constraints are <see cref="BoolExpr" /> trees
///     that must all be satisfied simultaneously.
/// </summary>
public sealed record CspInstance(
    IReadOnlyList<DecisionVariable> Variables,
    IReadOnlyList<BoolExpr> Constraints)
{
    /// <summary>
    ///     Creates an empty CSP instance with no variables or constraints.
    /// </summary>
    public static CspInstance Empty => new([], []);

    /// <summary>
    ///     Returns a new instance with <paramref name="variable" /> appended.
    /// </summary>
    public CspInstance WithVariable(DecisionVariable variable) =>
        this with { Variables = [.. Variables, variable] };

    /// <summary>
    ///     Returns a new instance with the variable named <paramref name="name" /> removed.
    /// </summary>
    public CspInstance WithoutVariable(string name) =>
        this with { Variables = Variables.Where(v => v.Name != name).ToList() };

    /// <summary>
    ///     Returns a new instance with <paramref name="constraint" /> appended.
    /// </summary>
    public CspInstance WithConstraint(BoolExpr constraint) =>
        this with { Constraints = [.. Constraints, constraint] };

    /// <summary>
    ///     Returns a new instance with the constraint at <paramref name="index" /> removed.
    /// </summary>
    public CspInstance WithoutConstraint(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constraints.Count);

        return this with { Constraints = Constraints.Where((_, i) => i != index).ToList() };
    }

    /// <summary>
    ///     Returns a new instance with the constraint at <paramref name="index" /> replaced.
    /// </summary>
    public CspInstance WithConstraintAt(int index, BoolExpr constraint)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constraints.Count);

        List<BoolExpr> updated = [.. Constraints];
        updated[index] = constraint;
        return this with { Constraints = updated };
    }
}