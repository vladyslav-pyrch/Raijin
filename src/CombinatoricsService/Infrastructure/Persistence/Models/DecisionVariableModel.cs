namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class DecisionVariableModel
{
    public int Id { get; set; }

    public Guid CombinatoricProblemId { get; set; }

    public string Name { get; set; } = null!;

    public string[] States { get; set; } = null!;
}