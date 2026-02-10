using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public sealed class Cryptominisat(IOptions<CryptominisatOptions> options) : ISatSolver
{
    private readonly CryptominisatOptions _options = options.Value;
    public async Task<string> SolveAsync(string dimacs, int? timeout = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filename = await WriteProblemInFile(dimacs);

        return await ExecCryptominisat(filename, timeout, cancellationToken);
    }

    private async Task<string> WriteProblemInFile(string dimacsSatProblem)
    {
        string fileName = RandomFileName();
        string filePath = Path.Combine(_options.FileExchangeLocalPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        await using var fileWriter = new StreamWriter(fileStream, Encoding.ASCII);

        await fileWriter.WriteAsync(dimacsSatProblem);

        return fileName;
    }

    private string RandomFileName()
    {
        string randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        return $"problem_{randomName}.cnf";
    }

    private async Task<string> ExecCryptominisat(string fileName, int? timeout, CancellationToken cancellationToken)
    {
        var commandBuilder = new StringBuilder();
        commandBuilder.Append($"exec -w {_options.FileExchangeContainerPath} {_options.ContainerName} cryptominisat5");
        commandBuilder.Append(" --verb 0");
        if (timeout.HasValue)
            commandBuilder.Append($" --maxtime {timeout.Value}");
        commandBuilder.Append($" {fileName}");

        var dockerProcessInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = commandBuilder.ToString(),
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

        return !string.IsNullOrWhiteSpace(error) ? throw new CryptominisatException(fileName, error) : output.Trim();
    }
}