namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class DecisionVariableModel
{
    public Guid CombinatoricProblemId { get; set; }
    
    public string Name { get; set; }
    
    public string[] States { get; set; }
}