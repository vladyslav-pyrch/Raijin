namespace Raijin.Application.Contracts;

public interface ISatProblemSent : IMessage
{
    public Guid SatProblemId { get; }

    public string Dimacs { get; }
}