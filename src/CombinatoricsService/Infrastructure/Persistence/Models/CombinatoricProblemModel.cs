namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class CombinatoricProblemModel
{
    public Guid Id { get; set; }

    public List<DecisionVariableModel> DecisionVariables { get; set; } = null!;

    public string[] Constraints { get; set; } = null!;

    public string Satisfiability { get; set; } = null!;

    public Dictionary<string, string> Solution { get; set; } = null!;
}