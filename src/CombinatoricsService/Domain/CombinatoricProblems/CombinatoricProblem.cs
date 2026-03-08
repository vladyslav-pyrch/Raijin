using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem(Guid id)
{
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];

    private readonly List<Constrain> _constrains = [];

    public Guid Id { get; } = id;

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constrain> Constrains => _constrains;

    public void AddDecisionVariable(string name, string[] states)
    {
        var variable = new DecisionVariable(name, states);
        _decisionVariables.Add(name, variable);

        StateNode[] stateNodes = states.Select(state => new StateNode(name, state)).ToArray();

        AddConstrain(formula: stateNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node)));

        foreach (StateNode currentNode in stateNodes)
        {
            StateNode[] otherNodes = stateNodes.Where(node => node != currentNode).ToArray();

            AddConstrain(formula:
                currentNode.Imply(otherNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node)).Negated())
            );
        }
    }

    public void AddConstrain(ExpressionNode formula)
    {
        var constrain = new Constrain(formula);

        foreach (StateNode stateNode in constrain.GetStateNodes())
        {
            // what exception should I throw here
            if (!_decisionVariables.TryGetValue(stateNode.DecisionVariableName, out DecisionVariable? variable))
                throw new ArgumentException(
                    $"Constrain contains a state node referring to decision variable that does not exist in the problem: {stateNode.DecisionVariableName}",
                    nameof(formula)
                );

            if (!variable.States.Contains(stateNode.DecisionVariableState))
                throw new ArgumentException(
                    $"Constrain contains a state node referring to a state that does not exist in the decision variable {variable.Name}: {stateNode.DecisionVariableState}",
                    nameof(formula)
                );
        }

        _constrains.Add(constrain);
    }

    public ExpressionNode ToFormula()
    {
        if (Constrains.Count == 0)
            throw new InvalidOperationException("A combinatoric problem must have at least one constrain to be converted to a formula.");
        
        return Constrains
            .Select(constrain => constrain.Formula)
            .Aggregate((acc, formula) => acc.And(formula));
    }
}