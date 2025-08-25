using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record SatVariable : ValueObject
{
    public SatVariable(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

        Id = id;
    }

    public int Id { get; }

    internal string ToDimacsString() => Id.ToString();
}