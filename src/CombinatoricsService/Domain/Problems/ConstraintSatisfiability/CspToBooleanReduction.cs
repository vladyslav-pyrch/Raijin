using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

internal static class CspToBooleanReduction
{
    internal static CspToBooleanReductionResult Apply(CspInstance csp)
    {
        ArgumentNullException.ThrowIfNull(csp);

        var allClauses = new List<BoolExpr>();
        var symbolTable = new Dictionary<DecisionVariableAssignment, BoolVar>();
        var auxiliaryVariables = new HashSet<BoolVar>();
        var decisionVariablesBoolVars = new HashSet<BoolVar>();

        foreach (DecisionVariable variable in csp.Variables)
        {
            BoolVar[] boolVars = variable.ToBoolVars();

            decisionVariablesBoolVars.UnionWith(boolVars);
            foreach (string state in variable.States)
                symbolTable[new DecisionVariableAssignment(variable.Name, state)] = variable.ToBoolVar(state);

            BoolExpr atLeastOneCondition = boolVars.Aggregate<BoolExpr>((acc, v) => acc.Or(v));
            BoolExpr atMostOneCondition = boolVars.Select<BoolVar, BoolExpr>(boolVar => boolVar.Imply(
                    boolVars.Where(bv => bv != boolVar).Aggregate<BoolExpr>((acc, bv) => acc.Or(bv)).Negated()
                )
            ).Aggregate((acc, v) => acc.And(v));
            BoolExpr exactlyOneCondition = atLeastOneCondition.And(atMostOneCondition);
            
            allClauses.Add(exactlyOneCondition);
        }

        foreach (BoolExpr constraint in csp.Constraints)
        {
            auxiliaryVariables.UnionWith(constraint.GetVariables());
            allClauses.Add(constraint);
        }

        auxiliaryVariables.ExceptWith(decisionVariablesBoolVars);
        BoolExpr finalExpr = allClauses.Aggregate((acc, c) => acc.And(c));

        return new CspToBooleanReductionResult(
            new BooleanProblemInstance(finalExpr),
            symbolTable,
            auxiliaryVariables.ToList()
        );
    }
}