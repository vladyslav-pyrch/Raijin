namespace Raijin.Application.Contracts;

public interface ISatProblemSubmitted : IMessage
{
    public Guid SatProblemId { get; }

    public string Dimacs { get; }
}