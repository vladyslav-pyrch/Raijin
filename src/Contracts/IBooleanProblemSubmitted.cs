namespace Raijin.Application.Contracts;

public interface IBooleanProblemSubmitted : IMessage
{
    public Guid BooleanProblemId { get; }

    public string BooleanFormula { get; }
}