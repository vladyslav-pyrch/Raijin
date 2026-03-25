using MassTransit;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Middleware;

internal sealed class CorrelationContextMiddleware(
    ICorrelationContextAccessor accessor,
    ILogger<CorrelationContextMiddleware> logger
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        Guid initiatorId = NewId.NextGuid();
        string? userId = null;

        if (httpContext.User.Identity is { IsAuthenticated: true })
            userId = httpContext.User.Identity.Name;

        accessor.CorrelationContext = new CorrelationContext(initiatorId, initiatorId, userId);

        using IDisposable? scope = logger.BeginScope(
            "InitiatorId: {InitiatorId}, CorrelationId: {CorrelationId}, UserId: {UserId}",
            accessor.CorrelationContext.InitiatorId,
            accessor.CorrelationContext.CorrelationId,
            accessor.CorrelationContext.UserId
        );

        await next(httpContext);
    }
}