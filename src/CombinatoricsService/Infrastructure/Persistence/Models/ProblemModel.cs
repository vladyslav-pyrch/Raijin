using System.Text.Json;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class ProblemModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public JsonDocument Instance { get; set; }

    public JsonDocument Solution { get; set; }

    public Guid? SatRunId { get; set; }
}