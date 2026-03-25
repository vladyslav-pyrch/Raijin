namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class BooleanProblemModel
{
    public Guid Id { get; set; }

    public string Formula { get; set; } = null!;

    public string Satisfiability { get; set; } = null!;

    public Dictionary<string, bool> Solution { get; set; } = null!;
}