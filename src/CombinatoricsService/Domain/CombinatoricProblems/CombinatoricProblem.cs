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
    }

    public void AddConstrain(string formula) => AddConstrain(new Constraint(formula));

    public void AddConstrain(ExpressionNode expression) => AddConstrain(new Constraint(expression));

    public ExpressionNode ToFormula()
    {
        if (Constraints.Count == 0)
            throw new InvalidOperationException(
                "A combinatoric problem must have at least one constraint to be converted to a formula.");
        
        ExpressionNode decisionVariableConstraints = DecisionVariables
            .Select(variable => AddAtLeastOneStateConstraint(variable).And(AddAtMostOneStateConstraint(variable)))
            .Aggregate((acc, constraint) => acc.And(constraint));

        return Constraints
            .Select(constraint => constraint.Expression)
            .Aggregate((acc, expression) => acc.And(expression))
            .And(decisionVariableConstraints);
    }

    private ExpressionNode AddAtLeastOneStateConstraint(DecisionVariable decisionVariable)
    {
        Variable[] variables = decisionVariable.ToVariables();

        return variables.Aggregate<ExpressionNode>((acc, node) => acc.Or(node));
    }

    private ExpressionNode AddAtMostOneStateConstraint(DecisionVariable decisionVariable)
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
    
    public static CombinatoricProblem Rehydrate(Guid id, IEnumerable<(string Name, string[] States)> decisionVariables, IEnumerable<string> constraints)
    {
        var combinatoricProblem = new CombinatoricProblem(id);

        foreach ((string name, string[] states) in decisionVariables)
            combinatoricProblem._decisionVariables.Add(name, new DecisionVariable(name, states));
        
        foreach (string constraint in constraints)
            combinatoricProblem._constrains.Add(new Constraint(constraint));

        return combinatoricProblem;
    }
}