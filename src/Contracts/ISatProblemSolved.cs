namespace Raijin.Application.Contracts;

public interface ISatProblemSolved : IMessage
{
    public Guid ProblemId { get; }

    public IEnumerable<int> Solution { get; }

    public string Satisfiability { get; }
}