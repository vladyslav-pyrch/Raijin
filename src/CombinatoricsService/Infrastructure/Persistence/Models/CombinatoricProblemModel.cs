namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class CombinatoricProblemModel
{
    public Guid Id { get; set; }

    public List<DecisionVariableModel> DecisionVariables { get; set; }

    public string[] Constraints { get; set; }

    public string Satisfiability { get; set; }

    public Dictionary<string, string>? Solution { get; set; }
}