using System.ComponentModel.DataAnnotations;

namespace Raijin.SatSolver.Api.Endpoints.V1.Sat.SubmitSatProblem;

public class SubmitSatProblemRequest
{
    [Required]
    public string Dimacs { get; set; }
}