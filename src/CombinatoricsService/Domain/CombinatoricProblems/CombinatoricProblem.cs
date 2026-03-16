using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem(Guid id)
{
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];

    private readonly List<Constraint> _constrains = [];

    public Guid Id { get; } = id;

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constraint> Constraints => _constrains;

    public void AddDecisionVariable(string name, string[] states)
    {
        var decisionVariable = new DecisionVariable(name, states);
        _decisionVariables.Add(name, decisionVariable);

        AddAtLeastOneStateConstraint(decisionVariable);
        AddAtMostOneStateConstraint(decisionVariable);
    }

    public void AddConstrain(string formula) => AddConstrain(new Constraint(formula));

    public void AddConstrain(ExpressionNode expression) => AddConstrain(new Constraint(expression));

    public ExpressionNode ToFormula()
    {
        if (Constraints.Count == 0)
            throw new InvalidOperationException(
                "A combinatoric problem must have at least one constraint to be converted to a formula.");

        return Constraints
            .Select(constraint => constraint.Expression)
            .Aggregate((acc, expression) => acc.And(expression));
    }

    private void AddAtLeastOneStateConstraint(DecisionVariable decisionVariable)
    {
        Variable[] variables = decisionVariable.ToVariables();

        AddConstrain(expression: variables.Aggregate<ExpressionNode>((acc, node) => acc.Or(node)));
    }

    private void AddAtMostOneStateConstraint(DecisionVariable decisionVariable)
    {
        Variable[] variables = decisionVariable.ToVariables();

        foreach (Variable currentVariable in variables)
        {
            Variable[] otherVariables = variables.Where(node => node != currentVariable).ToArray();

            AddConstrain(expression: currentVariable.Imply(
                otherVariables.Aggregate<ExpressionNode>((acc, variable) => acc.Or(variable)
                ).Negated()
            ));
        }
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
}