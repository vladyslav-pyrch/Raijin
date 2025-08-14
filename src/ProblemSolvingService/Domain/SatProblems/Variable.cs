using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record Variable : ValueObject
{
    public Variable(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

        Id = id;
    }

    public int Id { get; }

    internal string ToDimacsString() => Id.ToString();
}