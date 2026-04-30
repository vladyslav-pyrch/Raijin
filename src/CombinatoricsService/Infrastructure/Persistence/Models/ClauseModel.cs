namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class ClauseModel
{
    public Guid Id { get; set; }

    public Guid ProblemId { get; set; }

    public int[] Literals { get; set; } = [];
}