namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal sealed class CadicalSolveOptions
{
    public const string SectionName = "Cadical";

    public string FileName { get; set; } = "cadical";

    public int? TimeoutSeconds { get; set; }

    public string? CnfDirectory { get; set; }
}
