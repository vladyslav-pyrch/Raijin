using System.Diagnostics;
using System.Text;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Infrastructure.Solver;

public class CryptominisatSolver : ISatSolver
{
    public Task<int[]> Solve(SatProblem problem, CancellationToken cancellationToken)
        => InternalSolveAsync(problem, timeout: null, cancellationToken);

    public Task<int[]> Solve(SatProblem problem, int timeout, CancellationToken cancellationToken)
        => InternalSolveAsync(problem, timeout, cancellationToken);

    private async Task<int[]> InternalSolveAsync(SatProblem problem, int? timeout, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = await WriteProblemInFile(problem.Dimacs);

        string result = await CallCryptominisat(filePath, timeout, cancellationToken);

        return ParseCryptominisatResult(result);
    }

    private int[] ParseCryptominisatResult(string result)
    {
        if (result.StartsWith("s UNSATISFIABLE"))
            return [];

        if (!result.StartsWith("s SATISFIABLE"))
            throw new CryptominisatException($"Unexpected output from cryptominisat: {result}");

        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        string[] solutionLines = lines.Where(line => line.StartsWith("v ")).ToArray();

        if (solutionLines .Length == 0)
            throw new CryptominisatException($"No solution line found in cryptominisat output: {result}");

        IEnumerable<string> literals = solutionLines.SelectMany(solutionLine => solutionLine[2..].Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return literals.SkipLast(1).Select(int.Parse).ToArray();
    }

    private async Task<string> CallCryptominisat(string fileName, int? timeout, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = CreateCryptominisatProcessStartInfo(fileName, timeout);
        process.Start();

        try
        {
            string output = await GetOutputFromProcess(process, cancellationToken);
            return output;
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            throw;
        }
    }

    private ProcessStartInfo CreateCryptominisatProcessStartInfo(string filePath, int? timeout) => new()
    {
        FileName = "cryptominisat5",
        Arguments = timeout.HasValue ? $"--verb 0 --maxtime {timeout.Value} {filePath}" : $"--verb 0 {filePath}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    private async Task<string> GetOutputFromProcess(Process process, CancellationToken cancellationToken)
    {
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return !string.IsNullOrWhiteSpace(error) ? throw new CryptominisatException(error) : output.Trim();
    }

    private async Task<string> WriteProblemInFile(string dimacsSatProblem)
    {
        string fileName = RandomFileName();
        string filePath = Path.Combine("./problems", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "./problems");

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        await using var fileWriter = new StreamWriter(fileStream, Encoding.ASCII);

        await fileWriter.WriteAsync(dimacsSatProblem);

        return filePath;
    }

    private string RandomFileName()
    {
        string randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        return $"problem_{randomName}.cnf";
    }
}