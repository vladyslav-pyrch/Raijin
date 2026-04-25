namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class ClauseModel
{
    public Guid Id { get; set; }

    public Guid SatEncodingId { get; set; }

    public int[] Literals { get; set; } = [];
}