using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal sealed class CryptominisatCli(
    IOptions<CryptominisatSolveOptions> options,
    ILogger<CryptominisatCli> logger) : ICryptominisatCli
{
    private readonly CryptominisatSolveOptions _options = options.Value;

    public async Task<Result<string>> ExecuteAsync(
        string cnfFilePath,
        CryptominisatArgumentsBuilder arguments,
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
            logger.LogDebug("Starting CryptoMiniSat process for file: {FilePath}", cnfFilePath);
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
                logger.LogWarning("CryptoMiniSat stderr: {Error}", error);

            // Only exit codes 10 (SAT) and 20 (UNSAT) are valid solver results
            if (process.ExitCode is not (10 or 20))
            {
                logger.LogWarning("CryptoMiniSat exited with unexpected code {ExitCode}", process.ExitCode);
                return Result.Fail<string>($"CryptoMiniSat process failed with exit code {process.ExitCode}");
            }

            logger.LogDebug("CryptoMiniSat completed successfully for file: {FilePath}", cnfFilePath);
            return Result.Ok(output.Trim());
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("CryptoMiniSat execution cancelled for file: {FilePath}", cnfFilePath);
            await KillProcessAsync(process);
            return Result.Fail<string>("CryptoMiniSat execution was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing CryptoMiniSat for file: {FilePath}", cnfFilePath);
            await KillProcessAsync(process);
            return Result.Fail<string>($"Error executing CryptoMiniSat: {ex.Message}");
        }
    }

    private Process CreateProcess(string cnfFilePath, CryptominisatArgumentsBuilder arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _options.FileName,
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

    private static string BuildArguments(string cnfFilePath, CryptominisatArgumentsBuilder arguments)
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
                logger.LogWarning(ex, "Failed to kill CryptoMiniSat process");
            }
        }
    }
}