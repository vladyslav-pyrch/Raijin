using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace Njinx.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class Cryptominisat(IOptions<CryptominisatOptions> options)
{
    private readonly CryptominisatOptions _options = options.Value;

    public async Task<string> Solve(string dimacsSatProblem, CancellationToken cancellationToken = default)
    {
        if (!await IsContainerRunning(cancellationToken))
            throw new InvalidOperationException($"Container \"{_options.ContainerName}\" is not running or does not exist");

        string filename = await WriteProblemInFile(dimacsSatProblem);

        return await ExecCryptominisat(filename, cancellationToken);
    }

    private async Task<string> WriteProblemInFile(string dimacsSatProblem)
    {
        var fileName = $"problem_{Path.GetRandomFileName()}.cnf";
        string filePath = Path.Combine(_options.FileExchangeDirectory, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        await using var fileWriter = new StreamWriter(fileStream, Encoding.ASCII);

        await fileWriter.WriteAsync(dimacsSatProblem);

        return fileName;
    }

    private async Task<bool> IsContainerRunning(CancellationToken cancellationToken)
    {

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"inspect -f \"{{{{.State.Running}}}}\" {_options.ContainerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            string output = (await process.StandardOutput.ReadToEndAsync(cancellationToken)).Trim();
            await process.WaitForExitAsync(cancellationToken);

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

    private async Task<string> ExecCryptominisat(string fileName, CancellationToken cancellationToken)
    {
        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"exec -w {_options.WorkingDirectory} {_options.ContainerName} {_options.RunCommand} --verb 0 --maxtime {_options.TimeoutSeconds} {fileName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = dockerProcessInfo;
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(error))
            throw new CryptominisatException(fileName, error);

        return output.Trim();
    }
}