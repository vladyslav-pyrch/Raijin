namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public sealed class CryptominisatOptions
{
    public string ContainerName { get; set; } = null!;

    public string FileExchangeLocalPath { get; set; } = null!;

    public string FileExchangeContainerPath { get; set; } = null!;
}