namespace Raijin.Application.Contracts;

public interface ISatProblemSubmitted
{
    public Guid SatProblemId { get;  }

    public string Dimacs { get; }
}