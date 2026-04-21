using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public sealed record CspInstance(
    IReadOnlyList<DecisionVariable> Variables,
    IReadOnlyList<BoolExpr> Constraints
    ) : Instance
{
    public override string ProblemType() => ProblemTypes.ConstraintSatisfiabilityProblem;


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


    internal override SatEncoding ReduceToSat() =>
        CspToBooleanReduction.Apply(this).Instance.ReduceToSat();

    internal override Solution InterpretSolution(IReadOnlyList<int> assignments)
    {
        var processedAssignments = assignments.Select(i => new
        {
            Index = Math.Abs(i),
            Value = i > 0
        }).ToArray();
        
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(this);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspToBoolResult.Instance);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);
        
        Dictionary<int, DecisionVariableStateAssignment> invertedSymbolTable = cspToBoolResult.SymbolTable
            .ToDictionary(kvp => kvp.Key, kvp => dimacsResult.SymbolTable[tseitinResult.SymbolTable[kvp.Value]])
            .Invert();
        Dictionary<int, BoolVar> invertedAuxSymbolTable = cspToBoolResult.AuxiliaryVariables
            .ToDictionary(boolVar => dimacsResult.SymbolTable[tseitinResult.SymbolTable[boolVar]]);

        List<DecisionVariableStateAssignment> configuration = processedAssignments
            .Where(assignment => assignment.Value)
            .Where(assignment => invertedSymbolTable.ContainsKey(assignment.Index))
            .Select(assignment => invertedSymbolTable[assignment.Index])
            .ToList();
        List<BooleanVariableAssignment> auxiliaryAssignments = processedAssignments
            .Where(assignment => invertedAuxSymbolTable.ContainsKey(assignment.Index))
            .Select(assignment => new BooleanVariableAssignment(invertedAuxSymbolTable[assignment.Index], assignment.Value))
            .ToList();

        return new CspSolution(configuration, auxiliaryAssignments);
    }
}
