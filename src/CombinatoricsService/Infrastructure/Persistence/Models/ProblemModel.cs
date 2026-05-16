using System.Text.Json;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class ProblemModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string ProblemType { get; set; }

    public string? Solver { get; set; }

    public required JsonDocument Instance { get; set; }

    public JsonDocument? Solution { get; set; }

    public required string SolvingStatus { get; set; }

    public required string Satisfiability { get; set; }

    public int[] Assignment { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? StartedSolvingAt { get; set; }

    public DateTime? CompletedAt { get; set; }
    
    public TimeSpan? ElapsedTime { get; set; }

    public string? DimacsEncoding { get; set; }
}
