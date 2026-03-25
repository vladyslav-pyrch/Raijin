namespace Raijin.Application.Contracts;

public interface ISatProblemSolved : IMessage
{
    public Guid SatProblemId { get; }

    public int[] Solution { get; }
}