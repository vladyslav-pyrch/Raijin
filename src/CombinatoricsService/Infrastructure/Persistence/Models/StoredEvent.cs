using System.Text.Json;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

public class StoredEvent
{
    public Guid Id { get; set; }

    public Guid StreamId { get; set; }

    public string AggregateType { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public JsonDocument EventData { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}