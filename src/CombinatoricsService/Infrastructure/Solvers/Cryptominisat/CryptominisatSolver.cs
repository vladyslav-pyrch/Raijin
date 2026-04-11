using System.Text;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal sealed class CryptominisatSolver(
    ICryptominisatCli cli,
    IOptions<CryptominisatSolveOptions> options,
    ILogger<CryptominisatSolver> logger) : ISatSolver
{
    private readonly CryptominisatSolveOptions _options = options.Value;

    public async Task<Result<IReadOnlyList<int>>> Solve(
        SatEncoding satEncoding,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(satEncoding);
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Starting SAT solving with {VariableCount} variables and {ClauseCount} clauses",
            satEncoding.NumberOfVariables,
            satEncoding.NumberOfClauses);

        Result<string> filePathResult = await WriteCnfFileAsync(satEncoding, cancellationToken);
        if (filePathResult.IsFailed)
            return Result.Fail<IReadOnlyList<int>>(filePathResult.Errors);

        string filePath = filePathResult.Value;

        try
        {
            var arguments = new CryptominisatArgumentsBuilder().WithVerbosity(0);

            if (_options.TimeoutSeconds.HasValue)
                arguments = arguments.WithMaxTime(_options.TimeoutSeconds.Value);

            Result<string> executionResult = await cli.ExecuteAsync(
                filePath,
                arguments,
                cancellationToken);

            if (executionResult.IsFailed)
            {
                logger.LogWarning("CryptoMiniSat execution failed: {Errors}", executionResult.Errors);
                return Result.Fail<IReadOnlyList<int>>(executionResult.Errors);
            }

            Result<IReadOnlyList<int>> parseResult = ParseSolution(executionResult.Value);
            if (parseResult.IsFailed)
                logger.LogError("Failed to parse CryptoMiniSat output: {Errors}", parseResult.Errors);
            else
                logger.LogInformation("SAT solving completed successfully");

            return parseResult;
        }
        finally
        {
            CleanupCnfFile(filePath);
        }
    }

    private async Task<Result<string>> WriteCnfFileAsync(
        SatEncoding satEncoding,
        CancellationToken cancellationToken)
    {
        try
        {
            string fileName = $"problem_{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}.cnf";
            string filePath = Path.Combine(_options.CnfDirectory ?? "./problems", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "./problems");

            string dimacs = satEncoding.ToDimacs();

            await using var fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            await using var writer = new StreamWriter(fileStream, Encoding.ASCII);

            await writer.WriteAsync(dimacs.AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);

            logger.LogDebug("CNF file written to: {FilePath}", filePath);
            return Result.Ok(filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write CNF file");
            return Result.Fail<string>($"Failed to write CNF file: {ex.Message}");
        }
    }

    private Result<IReadOnlyList<int>> ParseSolution(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return Result.Fail<IReadOnlyList<int>>("Empty output from CryptoMiniSat");

        string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Any(line => line.StartsWith("s UNSATISFIABLE")))
        {
            logger.LogInformation("Problem is UNSATISFIABLE");
            return Result.Ok<IReadOnlyList<int>>(Array.Empty<int>());
        }

        if (!lines.Any(line => line.StartsWith("s SATISFIABLE")))
            return Result.Fail<IReadOnlyList<int>>($"Unexpected output format: {output}");

        string[] solutionLines = lines.Where(line => line.StartsWith("v ")).ToArray();

        if (solutionLines.Length == 0)
            return Result.Fail<IReadOnlyList<int>>("No solution variables found in SATISFIABLE result");

        try
        {
            List<int> solution = solutionLines
                .SelectMany(line => line[2..].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(token => token != "0")
                .Select(int.Parse)
                .ToList();

            logger.LogDebug("Parsed solution with {Count} variable assignments", solution.Count);
            return Result.Ok<IReadOnlyList<int>>(solution);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse solution literals");
            return Result.Fail<IReadOnlyList<int>>($"Failed to parse solution: {ex.Message}");
        }
    }

    private void CleanupCnfFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                logger.LogDebug("Cleaned up CNF file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete CNF file: {FilePath}", filePath);
        }
    }
}
