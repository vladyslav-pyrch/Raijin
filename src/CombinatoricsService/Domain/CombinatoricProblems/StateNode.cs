using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public record StateNode : Variable
{
    public StateNode(string decisionVariableName, string decisionVariableState) :
        base($"{decisionVariableName}_is_{decisionVariableState}")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionVariableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionVariableState);
        
        DecisionVariableName = decisionVariableName;
        DecisionVariableState = decisionVariableState;
    }
    
    public string DecisionVariableName { get; }
    
    public string DecisionVariableState { get; }
}