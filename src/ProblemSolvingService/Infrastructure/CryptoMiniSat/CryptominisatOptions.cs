namespace Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class CryptominisatOptions
{
    public string ContainerName { get; set; } = null!;

    public string FileExchangeLocalPath { get; set; } = null!;

    public string FileExchangeContainerPath { get; set; } = null!;

    public int TimeoutSeconds { get; set; }
}