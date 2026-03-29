using System.Text.Json;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

public class SatEncodingModel
{
    public Guid Id { get; set; }

    public Guid ProblemId { get; set; }

    public string Dimacs { get; set; }

    public JsonDocument VariableMap { get; set; }
}