namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal sealed class CadicalSolveOptions
{
    public const string SectionName = "Cadical";

    public string ExecutableFilePath { get; set; } = "./cadical";

    public int? TimeoutSeconds { get; set; }
}