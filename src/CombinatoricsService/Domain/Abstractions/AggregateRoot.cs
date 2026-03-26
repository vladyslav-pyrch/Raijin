namespace Raijin.CombinatoricsService.Domain.Abstractions;

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _events = [];

    private readonly List<DomainEvent> _uncommitedEvents = [];

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<DomainEvent> Events => _events;

    public IReadOnlyCollection<DomainEvent> UncommitedEvents => _uncommitedEvents;

    public static TAggregate Rehydrate<TAggregate>(IEnumerable<DomainEvent> events)
        where TAggregate : AggregateRoot, new()
    {
        var aggregateRoot = new TAggregate();
        aggregateRoot.Rehydrate(events);
        return aggregateRoot;
    }

    public void Rehydrate(IEnumerable<DomainEvent> events)
    {
        _events.Clear();
        _uncommitedEvents.Clear();
        DomainEvent[] domainEvents = events as DomainEvent[] ?? events.ToArray();
        _events.AddRange(domainEvents);
        foreach (DomainEvent domainEvent in domainEvents)
            ApplyEvent(domainEvent);
    }

    public void ClearUncommitedEvents()
    {
        _uncommitedEvents.Clear();
    }

    protected void Enqueue(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
        _uncommitedEvents.Add(domainEvent);
        ApplyEvent(domainEvent);
    }

    private void ApplyEvent(DomainEvent domainEvent)
    {
        //Type eventType = domainEvent.GetType();
        //MethodInfo? method = GetType().GetMethod("Apply", BindingFlags.Instance, []);

        //if (method is null)
        //    throw new InvalidOperationException(
        //        $"No Apply method found on {GetType().Name} for event type {eventType.Name}");

        //method.Invoke(this, [domainEvent]);
        ((dynamic)this).Apply((dynamic)domainEvent);
    }
}