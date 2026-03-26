using Raijin.CombinatoricsService.Domain.Abstractions;
using Raijin.CombinatoricsService.Domain.BooleanProblems;
using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem : AggregateRoot
{
    private readonly List<Constraint> _constrains = [];
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];
    private readonly List<DecisionVariableAssignment> _solution = [];

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constraint> Constraints => _constrains;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public IReadOnlyList<DecisionVariableAssignment> Solution => _solution;

    public static CombinatoricProblem Create(Guid id)
    {
        var combinatoricProblem = new CombinatoricProblem();
        combinatoricProblem.Enqueue(new CombinatoricProblemCreated(id));
        return combinatoricProblem;
    }

    public void AddDecisionVariable(string name, string[] states)
    {
        var decisionVariable = new DecisionVariable(name, states);

        if (_decisionVariables.ContainsKey(name))
            throw new ArgumentException($"A decision variable with the name '{name}' already exists in the problem.",
                nameof(name));

        Enqueue(new DecisionVariableAdded(decisionVariable));
    }

    public void AddDecisionVariables(IEnumerable<(string Name, string[] States)> decisionVariables)
    {
        foreach ((string name, string[] states) in decisionVariables)
            AddDecisionVariable(name, states);
    }

    public void AddConstrain(string formula)
    {
        var constraint = new Constraint(formula);

        foreach (Variable variable in constraint.Expression.GetVariables())
        {
            string[] parts = variable.Name.Split("_is_");
            string decisionVariableName = parts[0];
            string decisionVariableState = parts[1];

            if (!_decisionVariables.TryGetValue(decisionVariableName, out DecisionVariable? decisionVariable))
                throw new ArgumentException(
                    $"The decision variable '{decisionVariableName}' is used in the constraints but is not defined in the decision variables",
                    nameof(constraint)
                );

            if (!decisionVariable.States.Contains(decisionVariableState))
                throw new ArgumentException(
                    $"The state '{decisionVariableState}' of decision variable '{decisionVariableName}' is used in the constraints but is not defined in the decision variables",
                    nameof(constraint)
                );
        }

        Enqueue(new ConstraintAdded(constraint));
    }

    public void AddConstrains(IEnumerable<string> formulas)
    {
        foreach (string formula in formulas)
            AddConstrain(formula);
    }

    public void SetSolution(IDictionary<string, string> solution)
    {
        ArgumentNullException.ThrowIfNull(solution);

        if (solution.Count == 0)
        {
            Enqueue(new CombinatoricProblemSolutionSet([], Satisfiability.Unsatisfiable));
            return;
        }

        if (solution.Keys.FirstOrDefault(variableName =>
                !_decisionVariables.ContainsKey(variableName)) is { } invalidVariableName)
            throw new ArgumentException(
                $"The solution contains a variable '{invalidVariableName}' that is not present in the problem.",
                nameof(solution)
            );

        if (_decisionVariables.Keys.FirstOrDefault(variableName =>
                !solution.ContainsKey(variableName)) is { } missingVariableName)
            throw new ArgumentException(
                $"The solution is missing a variable '{missingVariableName}' that is present in the problem.",
                nameof(solution)
            );

        if (solution.FirstOrDefault(kvp => !_decisionVariables[kvp.Key].States.Contains(kvp.Value))
            is { Key: { } decisionVariableName, Value: var invalidState })
            throw new ArgumentException(
                $"The solution assigns '{invalidState}' state that is not valid for the decision variable '{decisionVariableName}'.",
                nameof(solution)
            );

        List<DecisionVariableAssignment> decisionVariableAssignments = solution.Select(kvp =>
            new DecisionVariableAssignment(_decisionVariables[kvp.Key], kvp.Value)).ToList();

        Enqueue(new CombinatoricProblemSolutionSet(decisionVariableAssignments, Satisfiability.Satisfiable));
    }

    public void ResolveVariableAssignments(IDictionary<string, bool> variableAssignments)
    {
        ArgumentNullException.ThrowIfNull(variableAssignments);

        BooleanProblem booleanProblem = ReduceToBooleanProblem();
        booleanProblem.SetSolution(variableAssignments);

        Dictionary<string, string> solution = booleanProblem.Solution
            .Where(assignment => assignment.Value)
            .Select(assignment => assignment.Variable.Name.Split("_is_"))
            .ToDictionary(parts => parts[0], parts => parts[1]);

        SetSolution(solution);
    }

    public BooleanProblem ReduceToBooleanProblem()
    {
        ExpressionNode booleanReduction = DecisionVariables
            .Select(variable => BuildAtLeastOneStateConstraint(variable).And(BuildAtMostOneStateConstraint(variable)))
            .Aggregate((acc, constraint) => acc.And(constraint));


        if (Constraints.Count != 0)
            booleanReduction = booleanReduction.And(
                Constraints.Select(constraint => constraint.Expression)
                    .Aggregate((acc, expression) => acc.And(expression))
            );

        var booleanProblem = BooleanProblem.Create(Id, booleanReduction);

        return booleanProblem;
    }

    internal void Apply(CombinatoricProblemCreated created)
    {
        Id = created.Id;
        Satisfiability = Satisfiability.Unknown;
    }

    internal void Apply(ConstraintAdded constraintAdded)
    {
        _constrains.Add(constraintAdded.Constraint);
    }

    internal void Apply(DecisionVariableAdded decisionVariableAdded)
    {
        _decisionVariables.Add(
            decisionVariableAdded.DecisionVariable.Name,
            decisionVariableAdded.DecisionVariable
        );
    }

    internal void Apply(CombinatoricProblemSolutionSet solutionSet)
    {
        _solution.Clear();
        _solution.AddRange(solutionSet.Solution);
        Satisfiability = solutionSet.Satisfiability;
    }

    private static ExpressionNode BuildAtLeastOneStateConstraint(DecisionVariable decisionVariable)
    {
        Variable[] variables = decisionVariable.ToVariables();

        return variables.Aggregate<ExpressionNode>((acc, node) => acc.Or(node));
    }

    private static ExpressionNode BuildAtMostOneStateConstraint(DecisionVariable decisionVariable)
    {
        Variable[] variables = decisionVariable.ToVariables();

        IEnumerable<ExpressionNode> constraints = variables
            .Select(currentVariable => new
            {
                currentVariable,
                otherVariables = variables.Where(node => node != currentVariable).ToArray()
            })
            .Select(t => t.currentVariable.Imply(t.otherVariables
                .Aggregate<ExpressionNode>((acc, variable) => acc.Or(variable))
                .Negated()
            ));

        return constraints.Aggregate((acc, node) => acc.And(node));
    }
}