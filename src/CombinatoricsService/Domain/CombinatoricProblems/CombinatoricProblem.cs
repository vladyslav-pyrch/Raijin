using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem(Guid id)
{
    private readonly List<Constraint> _constrains = [];
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];

    public Guid Id { get; } = id;

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constraint> Constraints => _constrains;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public CombinatoricSolution? Solution { get; private set; }

    public static CombinatoricProblem Rehydrate(
        Guid id,
        IEnumerable<(string Name, string[] States)> decisionVariables,
        IEnumerable<string> constraints,
        Satisfiability satisfiability,
        IReadOnlyDictionary<string, string>? solution = null)
    {
        var combinatoricProblem = new CombinatoricProblem(id);

        foreach ((string name, string[] states) in decisionVariables)
            combinatoricProblem._decisionVariables.Add(name, new DecisionVariable(name, states));

        foreach (string constraint in constraints)
            combinatoricProblem._constrains.Add(new Constraint(constraint));

        combinatoricProblem.Satisfiability = satisfiability;

        if (solution is null)
            return combinatoricProblem;

        IEnumerable<DecisionVariableAssignment> assignments = solution
            .Select(kvp => new DecisionVariableAssignment(combinatoricProblem._decisionVariables[kvp.Key], kvp.Value));
        combinatoricProblem.Solution = new CombinatoricSolution(assignments);

        return combinatoricProblem;
    }

    public void AddDecisionVariable(string name, string[] states)
    {
        var decisionVariable = new DecisionVariable(name, states);
        _decisionVariables.Add(name, decisionVariable);
    }

    public void AddConstrain(string formula)
    {
        AddConstrain(new Constraint(formula));
    }

    public void ResolveSatSolution(SatSolution satSolution)
    {
        ArgumentNullException.ThrowIfNull(satSolution);

        BooleanProblem booleanProblem = ReduceToBooleanProblem();
        booleanProblem.ResolveSatSolution(satSolution);

        if (booleanProblem.Satisfiability == Satisfiability.Unsatisfiable)
        {
            Satisfiability = Satisfiability.Unsatisfiable;
            return;
        }

        IEnumerable<DecisionVariableAssignment> solution = booleanProblem.Solution!.Assignments
            .Where(assignment => assignment.Value)
            .Select(assignment => assignment.Variable)
            .Select(variable =>
            {
                string[] parts = variable.Name.Split("_is_");
                string decisionVariableName = parts[0];
                string decisionVariableState = parts[1];
                return new DecisionVariableAssignment(_decisionVariables[decisionVariableName], decisionVariableState);
            });

        Solution = new CombinatoricSolution(solution);
        Satisfiability = Satisfiability.Satisfiable;
    }

    public BooleanProblem ReduceToBooleanProblem()
    {
        if (Constraints.Count == 0)
            throw new InvalidOperationException(
                "A combinatoric problem must have at least one constraint to be reduced to to formula.");

        ExpressionNode decisionVariableConstraints = DecisionVariables
            .Select(variable => BuildAtLeastOneStateConstraint(variable).And(BuildAtMostOneStateConstraint(variable)))
            .Aggregate((acc, constraint) => acc.And(constraint));

        ExpressionNode booleanReduction = Constraints.Select(constraint => constraint.Expression)
            .Aggregate((acc, expression) => acc.And(expression))
            .And(decisionVariableConstraints);

        var booleanProblem = new BooleanProblem(Id);
        booleanProblem.SetExpression(booleanReduction);

        return booleanProblem;
    }

    private void AddConstrain(Constraint constraint)
    {
        CheckVariables(constraint);
        _constrains.Add(constraint);
    }

    private void CheckVariables(Constraint constraint)
    {
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