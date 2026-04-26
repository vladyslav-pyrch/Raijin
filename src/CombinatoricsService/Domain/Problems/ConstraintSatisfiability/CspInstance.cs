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

    internal override SatEncoding ReduceToSat() =>
        CspToBooleanReduction.Apply(this).Instance.ReduceToSat();

    internal override IReadOnlyDictionary<string, int> GetVariableMap()
    {
        CspToBooleanReductionResult cspToBoolResult = CspToBooleanReduction.Apply(this);
        TseitinTransformResult tseitinResult = TseitinTransform.Apply(cspToBoolResult.Instance);
        DimacsReductionResult dimacsResult = DimacsReduction.Apply(tseitinResult.Instance);
        return dimacsResult.SymbolTable.ToDictionary(kvp => kvp.Key.Name, kvp => kvp.Value);
    }

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
