using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Domain.DomainEvents;

namespace Raijin.SatSolver.Infrastructure.DomainEvents;

public class DotNetDiDomainEventPublisher(IServiceProvider provider) : IDomainEventPublisher
{
    public Task Publish(IDomainEvent request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        Type requestHandlerType = typeof(IDomainEventHandler<>).MakeGenericType(requestType);

        object handler = provider.GetRequiredService(requestHandlerType);

        return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }
}