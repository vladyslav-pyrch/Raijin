namespace Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class CryptominisatOptions
{
    public CryptominisatOptions(){ }

    public CryptominisatOptions(string containerName, string workingDirectory, string fileExchangeDirectory, string runCommand, int timeoutSeconds)
    {
        ContainerName = containerName;
        WorkingDirectory = workingDirectory;
        FileExchangeDirectory = fileExchangeDirectory;
        RunCommand = runCommand;
        TimeoutSeconds = timeoutSeconds;
    }


    public string ContainerName { get; set; } = null!;
    public string WorkingDirectory { get; set; } = null!;
    public string FileExchangeDirectory { get; set; } = null!;
    public string RunCommand { get; set; } = null!;
    public int TimeoutSeconds { get; set; }

    public void Deconstruct(out string containerName, out string workingDirectory, out string fileExchangeDirectory, out string runCommand, out int timeoutSeconds)
    {
        containerName = ContainerName;
        workingDirectory = WorkingDirectory;
        fileExchangeDirectory = FileExchangeDirectory;
        runCommand = RunCommand;
        timeoutSeconds = TimeoutSeconds;
    }
}