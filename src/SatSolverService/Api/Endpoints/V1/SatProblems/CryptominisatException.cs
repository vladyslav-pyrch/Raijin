namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public sealed class CryptominisatException(string inputFileName, string? message = null) : Exception(message)
{
    public string InputFileName { get; } = inputFileName;
}