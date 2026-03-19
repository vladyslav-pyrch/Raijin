namespace Raijin.Application.Contracts;

public interface ICombinatoricProblemSolved : IMessage
{
    public string CombinatoricProblemId { get; }
    
    public IDictionary<string, string> Solution { get; }
}