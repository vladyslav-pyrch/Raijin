namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal sealed class CryptominisatSolveOptions
{
    public const string SectionName = "Cryptominisat";

    public string ExecutableFilePath { get; set; } = "./cryptominisat5";

    public int? TimeoutSeconds { get; set; }
}