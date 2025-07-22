using Microsoft.Extensions.DependencyInjection;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Cqrs;

public sealed class CommandQueryDispatcher(IServiceProvider provider) : ICommandDispatcher, IQueryDispatcher
{
    async Task ICommandDispatcher.Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand>>();

        await handler.Handle(command, cancellationToken);
    }

    async Task<TResult> ICommandDispatcher.Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        return await handler.Handle(command, cancellationToken);
    }

    async Task<TResult> IQueryDispatcher.Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return await handler.Handle(query, cancellationToken);
    }
}