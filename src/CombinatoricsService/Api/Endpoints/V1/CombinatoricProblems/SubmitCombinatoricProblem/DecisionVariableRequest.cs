using System.ComponentModel.DataAnnotations;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.CombinatoricProblems.SubmitCombinatoricProblem;

public class DecisionVariableRequest
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string[] States { get; set; }
}