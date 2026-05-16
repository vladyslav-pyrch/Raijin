using System.Diagnostics;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal sealed class CadicalSolver(
    ICadicalCli cli,
    IOptions<CadicalSolveOptions> options,
    ILogger<CadicalSolver> logger) : ISatSolver
{
    private readonly CadicalSolveOptions _options = options.Value;

    public string Name => "cadical";

    public async Task<Result<SatSolverResult>> Solve(
        SatEncoding satEncoding,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(satEncoding);
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "SAT solver started. Solver={Solver} VariableCount={VariableCount} ClauseCount={ClauseCount}",
            Name,
            satEncoding.NumberOfVariables,
            satEncoding.NumberOfClauses);
        var stopwatch = Stopwatch.StartNew();

        using CancellationTokenSource? timeoutCts = _options.TimeoutSeconds.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (timeoutCts is not null)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds!.Value));

        CancellationToken effectiveToken = timeoutCts?.Token ?? cancellationToken;

        Result<string> filePathResult = await WriteCnfFileAsync(satEncoding, effectiveToken);
        if (filePathResult.IsFailed)
            return Result.Fail<SatSolverResult>(filePathResult.Errors);

        string filePath = filePathResult.Value;

        try
        {
            CadicalArgumentsBuilder arguments = new CadicalArgumentsBuilder()
                .WithNoColors()
                .WithQuiet();

            Result<string> executionResult = await cli.ExecuteAsync(
                filePath,
                arguments,
                effectiveToken);

            if (executionResult.IsFailed)
            {
                bool wasTimeout = timeoutCts?.IsCancellationRequested == true
                                  && !cancellationToken.IsCancellationRequested;

                if (wasTimeout)
                {
                    logger.LogWarning(
                        "SAT solver timed out. Solver={Solver} TimeoutSeconds={TimeoutSeconds} ElapsedMs={ElapsedMs}",
                        Name,
                        _options.TimeoutSeconds,
                        stopwatch.ElapsedMilliseconds);
                    return Result.Fail<SatSolverResult>(new SolverTimeoutError(_options.TimeoutSeconds!.Value));
                }

                logger.LogWarning(
                    "SAT solver execution failed. Solver={Solver} ErrorCount={ErrorCount} ElapsedMs={ElapsedMs}",
                    Name,
                    executionResult.Errors.Count,
                    stopwatch.ElapsedMilliseconds);
                return Result.Fail<SatSolverResult>(executionResult.Errors);
            }

            Result<SatSolverResult> parseResult = ParseSolution(executionResult.Value);
            if (parseResult.IsFailed)
                logger.LogError(
                    "SAT solver output parse failed. Solver={Solver} ErrorCount={ErrorCount}",
                    Name,
                    parseResult.Errors.Count);
            else
                logger.LogInformation(
                    "SAT solver completed. Solver={Solver} Outcome={Outcome} ElapsedMs={ElapsedMs}",
                    Name,
                    parseResult.Value.Satisfiability,
                    stopwatch.ElapsedMilliseconds);

            return parseResult;
        }
        finally
        {
            timeoutCts?.CancelAfter(Timeout.InfiniteTimeSpan);
            CleanupCnfFile(filePath);
        }
    }

    private async Task<Result<string>> WriteCnfFileAsync(
        SatEncoding satEncoding,
        CancellationToken cancellationToken)
    {
        var fileName = $"problem_{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}.cnf";
        string filePath = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
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

            logger.LogDebug("CNF file written. Solver={Solver} FilePath={FilePath}", Name, filePath);
            return Result.Ok(filePath);
        }
        catch (Exception ex)
        {
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); }
                catch (Exception deleteEx)
                {
                    logger.LogWarning(deleteEx, "Failed to clean up partial CNF file. Solver={Solver} FilePath={FilePath}", Name, filePath);
                }
            }

            logger.LogError(ex, "Failed to write CNF file. Solver={Solver}", Name);
            return Result.Fail<string>($"Failed to write CNF file: {ex.Message}");
        }
    }

    private Result<SatSolverResult> ParseSolution(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return Result.Fail<SatSolverResult>("Empty output from CaDiCaL");

        string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Any(line => line.StartsWith("s UNSATISFIABLE")))
        {
            logger.LogDebug("SAT solver reported unsatisfiable outcome. Solver={Solver}", Name);
            return Result.Ok(new SatSolverResult(Satisfiability.Unsatisfiable, []));
        }

        if (!lines.Any(line => line.StartsWith("s SATISFIABLE")))
            return Result.Fail<SatSolverResult>($"Unexpected output format: {output}");

        string[] solutionLines = lines.Where(line => line.StartsWith("v ")).ToArray();

        if (solutionLines.Length == 0)
            return Result.Fail<SatSolverResult>("No solution variables found in SATISFIABLE result");

        try
        {
            List<int> solution = solutionLines
                .SelectMany(line => line[2..].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(token => token != "0")
                .Select(int.Parse)
                .ToList();

            logger.LogDebug("SAT solver solution parsed. Solver={Solver} AssignmentCount={AssignmentCount}", Name, solution.Count);
            return Result.Ok(new SatSolverResult(Satisfiability.Satisfiable, solution));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse solution literals. Solver={Solver}", Name);
            return Result.Fail<SatSolverResult>($"Failed to parse solution: {ex.Message}");
        }
    }

    private void CleanupCnfFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                logger.LogDebug("CNF file cleaned up. Solver={Solver} FilePath={FilePath}", Name, filePath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete CNF file. Solver={Solver} FilePath={FilePath}", Name, filePath);
        }
    }
}
