using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Raijin.CombinatoricsService.Application.Messaging.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> Handle(TRequest request,
        CancellationToken cancellationToken,
        Func<Task<Result<TResponse>>> next)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogDebug("Handling {RequestName}", requestName);
        var stopwatch = Stopwatch.StartNew();

        Result<TResponse> result = await next();

        stopwatch.Stop();

        if (result.IsSuccess)
            logger.LogDebug("Handled {RequestName} successfully in {ElapsedMs}ms", requestName,
                stopwatch.ElapsedMilliseconds);
        else
            logger.LogWarning("Handled {RequestName} with errors in {ElapsedMs}ms: {Errors}", requestName,
                stopwatch.ElapsedMilliseconds, string.Join("; ", result.Errors.Select(e => e.Message)));

        return result;
    }
}

public sealed class LoggingBehavior<TRequest>(
    ILogger<LoggingBehavior<TRequest>> logger
) : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result>> next)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogDebug("Handling {RequestName}", requestName);
        var stopwatch = Stopwatch.StartNew();

        Result result = await next();

        stopwatch.Stop();

        if (result.IsSuccess)
            logger.LogDebug("Handled {RequestName} successfully in {ElapsedMs}ms", requestName,
                stopwatch.ElapsedMilliseconds);
        else
            logger.LogWarning("Handled {RequestName} with errors in {ElapsedMs}ms: {Errors}", requestName,
                stopwatch.ElapsedMilliseconds, string.Join("; ", result.Errors.Select(e => e.Message)));

        return result;
    }
}
