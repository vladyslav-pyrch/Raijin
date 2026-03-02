namespace Raijin.Application.Contracts;

public interface ISatProblemSolved
{
    public Guid SatProblemId { get; }
    
    public int[] Solution { get; }
}