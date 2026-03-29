namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

public class SatRunModel
{
    public Guid Id { get; set; }

    public Guid ProblemId { get; set; }

    public string Satisfiability { get; set; }

    public string Status { get; set; }

    public int[] Assignment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}