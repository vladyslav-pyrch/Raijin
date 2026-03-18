namespace Raijin.Application.Contracts;

public interface ISatProblemSent : IMessage
{
    public string SatProblemId { get; }

    public string CombinatoricProblemId { get; }

    public string Dimacs { get; }
}

