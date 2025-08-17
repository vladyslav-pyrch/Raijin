using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class CryptominisatSatSolver(Cryptominisat cryptominisat) : ISatSolver
{
    public async Task<SatResult> Solve(SatProblem problem, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string result = await cryptominisat.Solve(problem.ToDimacsString(), cancellationToken);

        if (string.IsNullOrWhiteSpace(result))
            return SatResult.Indeterminate();

        string[] lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        return lines[0] switch
        {
            "s SATISFIABLE" => ParseSatisfiableResult(lines),
            "s UNSATISFIABLE" => SatResult.Unsolvable(),
            _ => SatResult.Indeterminate()
        };
    }

    private static SatResult ParseSatisfiableResult(string[] lines)
    {
        List<VariableAssignment> assignments = lines
            .Where(line => line.StartsWith("v "))
            .SelectMany(line => line[2..].Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .Where(value => value != 0)
            .Select(VariableAssignment.FromInteger)
            .ToList();

        return SatResult.Solvable(assignments);
    }
}