namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class SatEncodingModel
{
    public Guid Id { get; set; }

    public Guid ProblemId { get; set; }

    public ICollection<ClauseModel> Clauses { get; set; } = null!;
}