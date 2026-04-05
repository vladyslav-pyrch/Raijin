namespace Raijin.Application.Contracts;

public interface ISubmitSatProblem : IMessage
{
    public Guid ProblemId { get; }

    public IEnumerable<IEnumerable<int>> Clauses { get; }
}