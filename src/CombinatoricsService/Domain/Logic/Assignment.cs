namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed class Assignment(IEnumerable<VariableAssignment> Assignments)
{
//     public bool GetValue(Variable variable)
//     {
//         ArgumentNullException.ThrowIfNull(variable);
//
//         if (!_assignments.TryGetValue(variable, out bool value))
//             throw new KeyNotFoundException($"Variable '{variable.Name}' is not present in the assignment.");
//
//         return value;
//     }
//
//     public static Assignment FromSatSolution(
//         int[] solution,
//         IReadOnlyBijectiveDictionary<Variable, int> symbolTable)
//     {
//         ArgumentNullException.ThrowIfNull(solution);
//         ArgumentNullException.ThrowIfNull(symbolTable);
//
//         IReadOnlyBijectiveDictionary<int, Variable> inverse = symbolTable.Inverse;
//         Dictionary<Variable, bool> assignments = [];
//
//         foreach (int literal in solution)
//         {
//             int variableIndex = Math.Abs(literal);
//             bool isPositive = literal > 0;
//
//             if (inverse.TryGetValue(variableIndex, out Variable? variable))
//                 assignments[variable] = isPositive;
//         }
//
//         return new Assignment(assignments);
//     }
}

