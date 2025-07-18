namespace Njinx.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed record CryptominisatOptions(
    string ContainerName,
    string WorkingDirectory,
    string FileExchangeDirectory,
    string RunCommand,
    int TimeoutSeconds
    );