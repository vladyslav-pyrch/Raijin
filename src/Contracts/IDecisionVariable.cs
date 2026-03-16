namespace Raijin.Application.Contracts;

public interface IDecisionVariable
{
    public string Name { get; }
    
    public string[] States { get; }
}