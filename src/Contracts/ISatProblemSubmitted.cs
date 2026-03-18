namespace Raijin.Application.Contracts;

public interface ISatProblemSubmitted : IMessage
{
    public string SatProblemId { get; }

    public string? CombinatoricProblemId { get; }

    public string Dimacs { get; }
}