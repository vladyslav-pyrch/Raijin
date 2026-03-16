namespace Raijin.Application.Contracts;

public interface ICombinatoricProblemSubmitted
{
    public Guid CombinatoricProblemId { get; }
    
    public IDecisionVariable[] DecisionVariables { get; }
    
    public string[] Constraints { get; }
}