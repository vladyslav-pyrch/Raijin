using Raijin.CombinatoricsService.Domain.Abstractions;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IEventStore
{
    public Task<TAggregate?> GetById<TAggregate>(Guid id, CancellationToken cancellationToken)
        where TAggregate : AggregateRoot, new();

    public Task Save(AggregateRoot aggregate, CancellationToken cancellationToken);
}