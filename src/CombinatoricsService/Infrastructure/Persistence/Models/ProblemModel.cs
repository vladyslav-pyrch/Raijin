using System.Text.Json;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

public class ProblemModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ProblemKind { get; set; }

    public JsonDocument Instance { get; set; }

    public JsonDocument Solution { get; set; }

    public SatRunModel? SatRun { get; set; }

    public SatEncodingModel? SatEncoding { get; set; }
}