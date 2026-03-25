namespace Raijin.Application.Contracts;

public interface ICombinatoricProblemSolved : IMessage
{
    public Guid CombinatoricProblemId { get; }

    public IDictionary<string, string> Solution { get; }
}