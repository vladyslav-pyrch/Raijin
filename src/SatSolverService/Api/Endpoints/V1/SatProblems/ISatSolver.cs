namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public interface ISatSolver
{
    public Task<string> SolveAsync(string dimacs, int? timeout, CancellationToken cancellationToken);
}