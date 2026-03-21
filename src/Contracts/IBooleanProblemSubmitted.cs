namespace Raijin.Application.Contracts;

public interface IBooleanProblemSubmitted : IMessage
{
    public string BooleanProblemId { get; }

    public string BooleanFormula { get; }
}