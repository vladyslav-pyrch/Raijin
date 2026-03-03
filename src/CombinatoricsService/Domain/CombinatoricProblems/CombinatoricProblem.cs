using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public class CombinatoricProblem(Guid id)
{
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];

    private readonly Dictionary<string, Constrain> _constrains = [];

    public Guid Id { get; } = id;

    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();

    public IReadOnlyList<Constrain> Constrains => _constrains.Values.ToList();

    public void AddVariable(string variableName, string[] states)
    {
        var variable = new DecisionVariable(variableName, states);
        _decisionVariables.Add(variableName, variable);

        StateNode[] stateNodes = states.Select(state => new StateNode(variableName, state)).ToArray();

        AddConstrain(
            $"Must choose state for {variableName}",
            formula: stateNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node))
        );

        foreach (StateNode currentNode in stateNodes)
        {
            StateNode[] otherNodes = stateNodes.Where(node => node != currentNode).ToArray();

            AddConstrain(
                $"If state {currentNode.StateName} for variable {variableName} is chosen, then other states for the same variable cannot be chosen",
                formula: currentNode.Imply(
                    otherNodes.Aggregate<ExpressionNode>((acc, node) => acc.Or(node)).Negated()
                )
            );
        }
    }

    public void AddConstrain(string constrainName, ExpressionNode formula)
    {
        var constrain = new Constrain(constrainName, formula);

        foreach (StateNode stateNode in constrain.GetStateNodes())
        {
            // what exception should I throw here
            if (!_decisionVariables.TryGetValue(stateNode.VariableName, out DecisionVariable? variable))
                throw new ArgumentException($"Decision variable {stateNode.VariableName} does not exist");

            if (!variable.States.Contains(stateNode.StateName))
                throw new ArgumentException(
                    $"Decision variable {stateNode.VariableName} does not have state {stateNode.StateName}");
        }

        _constrains.Add(constrainName, constrain);
    }

    public ExpressionNode ToFormula() => Constrains
        .Select(constrain => constrain.Formula)
        .Aggregate((acc, formula) => acc.And(formula));
}