using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public class BooleanProblem
{
    private readonly Dictionary<string, Variable> _variables = [];

    public BooleanProblem(Guid id, string formula)
    {
        ArgumentNullException.ThrowIfNull(formula);

        Id = id;
        SetExpression(ExpressionParser.Parse(formula), formula);
    }

    public BooleanProblem(Guid id, ExpressionNode expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Id = id;
        SetExpression(expression, expression.ToString());
    }

    public Guid Id { get; }

    public string Formula { get; private set; } = null!;

    public ExpressionNode Expression { get; private set; } = null!;

    public IEnumerable<Variable> Variables => _variables.Values;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public BooleanProblemSolution Solution { get; private set; } = new([]);

    public static BooleanProblem Rehydrate(Guid id,
        string formula,
        Satisfiability satisfiability,
        IDictionary<string, bool> solution)
    {
        ArgumentNullException.ThrowIfNull(formula);

        var problem = new BooleanProblem(id, formula);

        foreach (Variable variable in problem.Expression.GetVariables())
            problem._variables.TryAdd(variable.Name, variable);

        problem.Satisfiability = satisfiability;
        problem.Solution = new BooleanProblemSolution(solution.Select(kvp => new VariableAssignment(
            problem._variables[kvp.Key],
            kvp.Value
        )));

        return problem;
    }

    public SatReduction ReduceToSat()
    {
        return Expression.TseitinTransform(Id);
    }

    public void SetSolution(IDictionary<string, bool> variableAssignments)
    {
        ArgumentNullException.ThrowIfNull(variableAssignments);

        if (variableAssignments.Count == 0)
        {
            Solution = new BooleanProblemSolution([]);
            Satisfiability = Satisfiability.Unsatisfiable;
            return;
        }

        if (variableAssignments.Keys.FirstOrDefault(variableName => !_variables.ContainsKey(variableName)) is
            { } invalidVariableName)
            throw new ArgumentException(
                $"The variable assignment contains a variable '{invalidVariableName}' that is not present in the problem.",
                nameof(variableAssignments)
            );

        if (_variables.Keys.FirstOrDefault(variableName => !variableAssignments.ContainsKey(variableName)) is
            { } missingVariableName)
            throw new ArgumentException(
                $"The variable assignment is missing a variable '{missingVariableName}' that is present in the problem.",
                nameof(variableAssignments)
            );

        Solution = new BooleanProblemSolution(variableAssignments.Select(kvp => new VariableAssignment(
            _variables[kvp.Key],
            kvp.Value
        )));
        Satisfiability = Satisfiability.Satisfiable;
    }

    public void ResolveSatSolution(int[] literals)
    {
        ArgumentNullException.ThrowIfNull(literals);

        SatSolution satSolution = new(literals);
        SatReduction satReduction = ReduceToSat();
        if (satSolution.NumberOfVariables != 0 && satSolution.NumberOfVariables != satReduction.NumberOfVariables)
            throw new ArgumentException(
                $"The SAT solution contains {satSolution.NumberOfVariables} variable(s) but the reduction expects {satReduction.NumberOfVariables}.",
                nameof(satSolution)
            );

        IReadOnlyBijectiveDictionary<int, Variable> inverse = satReduction.SymbolTable.Inverse;

        IDictionary<string, bool> assignments = satSolution.Literals
            .Select(literal => new { Index = Math.Abs(literal), Value = literal > 0 })
            .Where(arg => inverse.ContainsKey(arg.Index))
            .ToDictionary(
                arg => inverse[arg.Index].Name,
                arg => arg.Value
            );

        SetSolution(assignments);
    }

    private void SetExpression(ExpressionNode expression, string formula)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(formula);

        Formula = formula;
        Expression = expression;

        foreach (Variable variable in Expression.GetVariables())
            _variables.TryAdd(variable.Name, variable);
    }
}