using System.ComponentModel.DataAnnotations;
using Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.CombinatoricProblems.SubmitCombinatoricProblem;

public class SubmitCombinatoricProblemRequest
{
    [Required]
    public DecisionVariableRequest[] DecisionVariables { get; set; }
    
    [Required]
    public string[] Constraints { get; set; }
    
    public SubmitCombinatoricProblemCommand ToCommand() => new(
        DecisionVariables.Select(dv => new DecisionVariableDto(dv.Name, dv.States)).ToArray(),
        Constraints
    );
}