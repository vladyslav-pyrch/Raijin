using System.Diagnostics;
using Microsoft.Extensions.Options;
using Raijin.Constants;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Integration.CryptoMiniSat;

public sealed class CryptominisatDockerContainerFixture : IDisposable
{
    private readonly IOptions<CryptominisatOptions> _options;
    private readonly IOptions<CryptominisatOptions> _zeroTimeoutOptions;

    public CryptominisatDockerContainerFixture()
    {
        var containerName = Environment.GetEnvironmentVariable(EnvironmentVariables.Cryptominisat.ContainerName) ??
                            "cryptominisat";
        var fileExchangeLocalPath =
            Environment.GetEnvironmentVariable(EnvironmentVariables.Cryptominisat.FileExchangeLocalPath)
            ?? "../../../../../../src/file_exchange/cryptominisat";
        var fileExchangeContainerPath =
            Environment.GetEnvironmentVariable(EnvironmentVariables.Cryptominisat.FileExchangeContainerPath)
            ?? "/app/cryptominisat/problems";
        var timeout =
            Convert.ToInt32(Environment.GetEnvironmentVariable(EnvironmentVariables.Cryptominisat.TimeoutSeconds));

        _options = Microsoft.Extensions.Options.Options.Create(new CryptominisatOptions
        {
            ContainerName = containerName,
            FileExchangeLocalPath = fileExchangeLocalPath,
            FileExchangeContainerPath = fileExchangeContainerPath,
            TimeoutSeconds = timeout
        });
        _zeroTimeoutOptions = Microsoft.Extensions.Options.Options.Create(new CryptominisatOptions
        {
            ContainerName = containerName,
            FileExchangeLocalPath = fileExchangeLocalPath,
            FileExchangeContainerPath = fileExchangeContainerPath,
            TimeoutSeconds = 0
        });
    }

    public IOptions<CryptominisatOptions> Options => _options;

    public IOptions<CryptominisatOptions> ZeroTimeoutOptions => _zeroTimeoutOptions;

    public void Dispose() {}

    public bool DoesContainerExist()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"inspect {_options.Value.ContainerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("[]\n");
        }
        catch
        {
            return false;
        }
    }

    public bool IsContainerRunning()
    {

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"inspect -f \"{{{{.State.Running}}}}\" {_options.Value.ContainerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExitAsync();

            return output.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
