namespace Raijin.Application.Contracts;

public interface ISatProblemSolved : IMessage
{
    public string SatProblemId { get; }
    
    public int[] Solution { get; }
}