using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Middleware;

internal sealed class CorrelationContextMiddleware(
    ICorrelationContextAccessor accessor,
    ILogger<CorrelationContextMiddleware> logger
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var correlationId = Guid.CreateVersion7();
        string? userId = null;

        if (httpContext.User.Identity is { IsAuthenticated: true })
            userId = httpContext.User.Identity.Name;

        accessor.CorrelationContext = new CorrelationContext(correlationId, correlationId, userId);

        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = accessor.CorrelationContext.CorrelationId,
            ["CausationId"] = accessor.CorrelationContext.CausationId
        });

        await next(httpContext);
    }
}