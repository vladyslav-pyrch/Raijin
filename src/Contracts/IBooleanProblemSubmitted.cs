namespace Raijin.Application.Contracts;

public interface IBooleanProblemSubmitted : IMessage
{
    string BooleanProblemId { get; }

    string BooleanFormula { get; }
}