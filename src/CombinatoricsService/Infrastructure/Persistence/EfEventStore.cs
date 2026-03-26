using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Abstractions;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public sealed class EfEventStore(CombinatoricsServiceDbContext dbContext) : IEventStore
{
    public async Task<TAggregate?> GetById<TAggregate>(Guid id, CancellationToken cancellationToken)
        where TAggregate : AggregateRoot, new()
    {
        List<StoredEvent> storedEvents = await dbContext.StoredEvents.AsNoTracking()
            .Where(e => e.StreamId == id)
            .Where(e => e.AggregateType == typeof(TAggregate).AssemblyQualifiedName)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        if (storedEvents.Count == 0)
            return null;

        IEnumerable<DomainEvent> domainEvents = storedEvents.Select(e =>
        {
            var domainEventType = Type.GetType(e.EventType);
            if (domainEventType is null)
                throw new InvalidOperationException($"Could not find type {e.EventType}");

            if (e.EventData.Deserialize(domainEventType) is not DomainEvent domainEvent)
                throw new InvalidOperationException($"Could not deserialize event of type {e.EventType}");

            return domainEvent;
        }).ToArray();

        return AggregateRoot.Rehydrate<TAggregate>(domainEvents);
    }

    public async Task Save(AggregateRoot aggregate, CancellationToken cancellationToken)
    {
        foreach (DomainEvent domainEvent in aggregate.UncommitedEvents)
            dbContext.StoredEvents.Add(new StoredEvent
            {
                StreamId = aggregate.Id,
                AggregateType = aggregate.GetType().AssemblyQualifiedName!,
                EventType = domainEvent.GetType().AssemblyQualifiedName!,
                EventData = JsonSerializer.SerializeToDocument(domainEvent, domainEvent.GetType()),
                Timestamp = domainEvent.OccurredOn
            });

        aggregate.ClearUncommitedEvents();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}