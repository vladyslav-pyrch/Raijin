namespace Raijin.Application.Contracts;

public interface SatProblemSubmitted
{
    public Guid SatProblemId { get;  }

    public string Dimacs { get; }
}