namespace Raijin.Application.Contracts;

public interface IDecisionVariable : IMessage
{
    public string Name { get; }
    
    public string[] States { get; }
}