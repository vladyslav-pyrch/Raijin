using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal sealed class CadicalCli(
    IOptions<CadicalSolveOptions> options,
    ILogger<CadicalCli> logger) : ICadicalCli
{
    private readonly CadicalSolveOptions _options = options.Value;
    private const string Solver = "cadical";

    public async Task<Result<string>> ExecuteAsync(
        string cnfFilePath,
        CadicalArgumentsBuilder arguments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cnfFilePath);
        ArgumentNullException.ThrowIfNull(arguments);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(cnfFilePath))
            return Result.Fail<string>($"CNF file not found: {cnfFilePath}");

        using Process process = CreateProcess(cnfFilePath, arguments);

        try
        {
            logger.LogDebug("Starting solver process. Solver={Solver} FilePath={FilePath}", Solver, cnfFilePath);
            process.Start();

            // Read stdout and stderr concurrently to prevent deadlock
            // when the process fills its stderr OS buffer before stdout completes
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            await process.WaitForExitAsync(cancellationToken);

            // Log stderr as warning; do not throw — exit code determines success/failure
            if (!string.IsNullOrWhiteSpace(error))
                logger.LogWarning("Solver stderr emitted output. Solver={Solver} Error={Error}", Solver, error);

            // Only exit codes 10 (SAT) and 20 (UNSAT) are valid solver results
            if (process.ExitCode is not (10 or 20))
            {
                logger.LogWarning("Solver exited with unexpected code. Solver={Solver} ExitCode={ExitCode}", Solver, process.ExitCode);
                return Result.Fail<string>($"CaDiCaL process failed with exit code {process.ExitCode}");
            }

            logger.LogDebug("Solver process completed. Solver={Solver} FilePath={FilePath}", Solver, cnfFilePath);
            return Result.Ok(output.Trim());
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Solver execution cancelled. Solver={Solver} FilePath={FilePath}", Solver, cnfFilePath);
            await KillProcessAsync(process);
            return Result.Fail<string>("CaDiCaL execution was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing solver process. Solver={Solver} FilePath={FilePath}", Solver, cnfFilePath);
            await KillProcessAsync(process);
            return Result.Fail<string>($"Error executing CaDiCaL: {ex.Message}");
        }
    }

    private Process CreateProcess(string cnfFilePath, CadicalArgumentsBuilder arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutableFilePath,
            Arguments = BuildArguments(cnfFilePath, arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        return new Process
        {
            StartInfo = processStartInfo
        };
    }

    private static string BuildArguments(string cnfFilePath, CadicalArgumentsBuilder arguments)
    {
        string builtArgs = arguments.Build();
        string quotedPath = cnfFilePath.Contains(' ') ? $"\"{cnfFilePath}\"" : cnfFilePath;
        return string.IsNullOrEmpty(builtArgs)
            ? quotedPath
            : $"{builtArgs} {quotedPath}";
    }

    private async Task KillProcessAsync(Process process)
    {
        if (!process.HasExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to kill solver process. Solver={Solver}", Solver);
            }
        }
    }
}
