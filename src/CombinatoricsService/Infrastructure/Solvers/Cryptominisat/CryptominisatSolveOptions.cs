namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal sealed class CryptominisatSolveOptions
{
    public const string SectionName = "Cryptominisat";

    public string FileName { get; set; } = "cryptominisat5";

    public int? TimeoutSeconds { get; set; }

    public string? CnfDirectory { get; set; }
}