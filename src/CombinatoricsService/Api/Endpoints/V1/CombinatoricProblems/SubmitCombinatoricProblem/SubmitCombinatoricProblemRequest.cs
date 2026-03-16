using System.ComponentModel.DataAnnotations;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.CombinatoricProblems.SubmitCombinatoricProblem;

public class SubmitCombinatoricProblemRequest
{
    [Required]
    public DecisionVariableModel[] DecisionVariables { get; set; }
    
    [Required]
    public string[] Constraints { get; set; }
}