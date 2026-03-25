namespace Raijin.Application.Contracts;

public interface ICombinatoricProblemSubmitted : IMessage
{
    public Guid CombinatoricProblemId { get; }

    public IDecisionVariable[] DecisionVariables { get; }

    public string[] Constraints { get; }
}