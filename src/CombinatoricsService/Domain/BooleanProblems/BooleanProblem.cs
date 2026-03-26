using Raijin.CombinatoricsService.Domain.Abstractions;
using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public class BooleanProblem : AggregateRoot
{
    private readonly Dictionary<string, Variable> _variables = [];

    public string Formula { get; private set; } = null!;

    public ExpressionNode Expression { get; private set; } = null!;

    public IEnumerable<Variable> Variables => _variables.Values;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public IEnumerable<VariableAssignment> Solution { get; private set; } = [];

    public static BooleanProblem Create(Guid id, string formula)
    {
        var booleanProblem = new BooleanProblem();
        booleanProblem.Enqueue(new BooleanProblemCreated(id, formula));
        return booleanProblem;
    }

    public static BooleanProblem Create(Guid id, ExpressionNode expression)
    {
        var booleanProblem = new BooleanProblem();
        booleanProblem.Enqueue(new BooleanProblemCreated(id, expression.ToString()));
        return booleanProblem;
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
            Enqueue(new BooleanProblemSolutionSet([], Satisfiability.Unsatisfiable));
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

        List<VariableAssignment> assignments = variableAssignments.Select(kvp => new VariableAssignment(
            _variables[kvp.Key],
            kvp.Value
        )).ToList();
        Enqueue(new BooleanProblemSolutionSet(assignments, Satisfiability.Satisfiable));
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

    internal void Apply(BooleanProblemCreated domainEvent)
    {
        Id = domainEvent.Id;
        Formula = domainEvent.Formula;
        Expression = ExpressionParser.Parse(domainEvent.Formula);
        Satisfiability = Satisfiability.Unknown;

        foreach (Variable variable in Expression.GetVariables())
            _variables.TryAdd(variable.Name, variable);
    }

    internal void Apply(BooleanProblemSolutionSet domainEvent)
    {
        Satisfiability = domainEvent.Satisfiability;
        Solution = domainEvent.Solution;
    }
}