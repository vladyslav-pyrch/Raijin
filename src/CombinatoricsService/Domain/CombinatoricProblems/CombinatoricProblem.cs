using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem(Guid id)
{
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];

    private readonly Dictionary<string, Constrain> _constrains = [];

    public Guid Id { get; } = id;

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constrain> Constrains => _constrains.Values.ToList();

    public void AddDecisionVariable(string name, string[] states)
    {
        var variable = new DecisionVariable(name, states);
        _decisionVariables.Add(name, variable);

        StateNode[] stateNodes = states.Select(state => new StateNode(name, state)).ToArray();

        AddConstrain(
            $"Must choose state for {name}",
            formula: stateNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node))
        );

        foreach (StateNode currentNode in stateNodes)
        {
            StateNode[] otherNodes = stateNodes.Where(node => node != currentNode).ToArray();

            AddConstrain(
                $"If state {currentNode.DecisionVariableState} for variable {name} is chosen, then other states for the same variable cannot be chosen",
                formula: currentNode.Imply(
                    otherNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node)).Negated()
                )
            );
        }
    }

    public void AddConstrain(string name, ExpressionNode formula)
    {
        var constrain = new Constrain(name, formula);

        foreach (StateNode stateNode in constrain.GetStateNodes())
        {
            // what exception should I throw here
            if (!_decisionVariables.TryGetValue(stateNode.DecisionVariableName, out DecisionVariable? variable))
                throw new ArgumentException($"Decision variable {stateNode.DecisionVariableName} does not exist");

            if (!variable.States.Contains(stateNode.DecisionVariableState))
                throw new ArgumentException(
                    $"Decision variable {stateNode.DecisionVariableName} does not have state {stateNode.DecisionVariableState}");
        }

        _constrains.Add(name, constrain);
    }

    public ExpressionNode ToFormula() => Constrains
        .Select(constrain => constrain.Formula)
        .Aggregate((acc, formula) => acc.And(formula));
}