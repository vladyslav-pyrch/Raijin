using System.Diagnostics;
using Microsoft.Extensions.Options;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.CryptoMiniSat;

public sealed class CryptominisatDockerContainerFixture : IDisposable
{
    private readonly IOptions<CryptominisatOptions> _options;

    public CryptominisatDockerContainerFixture()
    {
        var containerName = Environment.GetEnvironmentVariable("CRYPTOMINISAT_CONTAINER_NAME") ?? "cryptominisat-test";
        var runCommand = Environment.GetEnvironmentVariable("CRYPTOMINISAT_RUN_COMMAND") ?? "cryptominisat5";
        var fileExchangeDirectory = Environment.GetEnvironmentVariable("CRYPTOMINISAT_FILE_EXCHANGE") ?? "./shared";
        var workingDirectory = Environment.GetEnvironmentVariable("CRYPTOMINISAT_WORKING_DIRECTORY") ?? "/app/cryptominisat/problems";
        var timeoutSeconds = 20;

        _options = Microsoft.Extensions.Options.Options.Create(new CryptominisatOptions(containerName, workingDirectory,
            fileExchangeDirectory, runCommand, timeoutSeconds));
    }

    public IOptions<CryptominisatOptions> Options => _options;

    public void Dispose() {}

    public void BuildDockerImage()
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"build -t {_options.Value.ContainerName} -f ../../../../../../src/cryptominisat/Dockerfile ../../../../../../src/cryptominisat/",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();
        process.WaitForExit();
    }

    public void RunContainer()
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"run -d --name {_options.Value.ContainerName} -v {_options.Value.FileExchangeDirectory}:{_options.Value.WorkingDirectory} {_options.Value.ContainerName}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();
        process.WaitForExit();
    }

    public void StopContainer()
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"stop {_options.Value.ContainerName}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();
        process.WaitForExit();
    }

    public void StartContainer()
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"start {_options.Value.ContainerName}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();
        process.WaitForExit();
    }

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

    public void StopAndRemoveContainer()
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"rm -f {_options.Value.ContainerName}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();
        process.WaitForExit();
    }
}
