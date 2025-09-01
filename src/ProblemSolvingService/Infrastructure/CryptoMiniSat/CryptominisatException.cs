namespace Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class CryptominisatException(string inputFileName, string? message = null) : Exception(message)
{
    public string InputFileName { get; } = inputFileName;
}