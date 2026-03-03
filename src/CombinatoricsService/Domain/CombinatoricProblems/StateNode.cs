using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public record StateNode : LeafNode
{
    public StateNode(string variableName, string stateName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(stateName);
        
        VariableName = variableName;
        StateName = stateName;
    }
    
    public string VariableName { get; }
    
    public string StateName { get; }
    
    public override string ToString() => $"{VariableName}_is_{StateName}";
}