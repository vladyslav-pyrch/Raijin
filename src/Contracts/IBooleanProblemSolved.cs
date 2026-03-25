namespace Raijin.Application.Contracts;

public interface IBooleanProblemSolved : IMessage
{
    public Guid BooleanProblemId { get; }

    public IDictionary<string, bool> Solution { get; }
}