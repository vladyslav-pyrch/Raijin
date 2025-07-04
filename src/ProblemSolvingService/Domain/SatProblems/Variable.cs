using Njinx.ProblemSolvingService.Domain.SharedKernel;

namespace Njinx.ProblemSolvingService.Domain.SatProblems;

public sealed record Variable : ValueObject
{
    public Variable(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

        Id = id;
    }

    public int Id { get; }
}