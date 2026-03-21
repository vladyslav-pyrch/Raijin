namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record CombinatoricSolution(IEnumerable<DecisionVariableAssignment> Assignments)
{
    // public static CombinatoricSolution FromAssignment(
    //     IReadOnlyList<DecisionVariable> decisionVariables,
    //     Assignment booleanAssignment)
    // {
    //     ArgumentNullException.ThrowIfNull(decisionVariables);
    //     ArgumentNullException.ThrowIfNull(booleanAssignment);
    //
    //     List<DecisionVariableAssignment> result = [];
    //
    //     foreach (DecisionVariable decisionVariable in decisionVariables)
    //     {
    //         List<string> trueStates = decisionVariable.ToVariables()
    //             .Select((variable, index) => new { Variable = variable, Index = index })
    //             .Where(arg => booleanAssignment.GetValue(arg.Variable))
    //             .Select(arg => decisionVariable.States[arg.Index])
    //             .ToList();
    //
    //         switch (trueStates.Count)
    //         {
    //             case 0:
    //                 throw new InvalidOperationException(
    //                     $"Decision variable '{decisionVariable.Name}' has no state assigned to true. " +
    //                     "This indicates a solver or encoding bug.");
    //             case > 1:
    //                 throw new InvalidOperationException(
    //                     $"Decision variable '{decisionVariable.Name}' has multiple states assigned to true: " +
    //                     $"[{string.Join(", ", trueStates)}]. This indicates a solver or encoding bug.");
    //             default:
    //                 result.Add(new DecisionVariableAssignment(decisionVariable, trueStates.Single()));
    //                 break;
    //         }
    //     }
    //
    //     return new CombinatoricSolution(result);
    // }
}

