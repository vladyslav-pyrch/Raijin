using System.ComponentModel.DataAnnotations;
using Raijin.SatSolver.Application.Features.SubmitSatProblem;

namespace Raijin.SatSolver.Api.Endpoints.V1.Sat.SubmitSatProblem;

public class SubmitSatProblemRequest
{
    [Required]
    public string Dimacs { get; set; }
    
    public SubmitSatProblemCommand ToCommand() => new(Dimacs);
}