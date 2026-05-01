using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Validation;

namespace Raijin.CombinatoricsService.Application.Messaging.Behaviors;

public sealed class ValidationBehavior<TRequest, TResult>(
    ILogger<ValidationBehavior<TRequest, TResult>> logger,
    IValidator<TRequest>? validator = null
) : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<Result<TResult>> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result<TResult>>> next)
    {
        if (validator is null)
            return await next();

        string requestName = typeof(TRequest).Name;
        logger.LogDebug("Validating {RequestName}", requestName);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for {RequestName} with {ErrorCount} error(s): {Errors}",
                requestName,
                validationResult.Errors.Count,
                string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            return Result.Fail(validationResult.ToValidationErrors());
        }

        logger.LogDebug("Validation passed for {RequestName}", requestName);
        return await next();
    }
}

public sealed class ValidationBehavior<TRequest>(
    ILogger<ValidationBehavior<TRequest>> logger,
    IValidator<TRequest>? validator = null
) : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result>> next)
    {
        if (validator is null)
            return await next();

        string requestName = typeof(TRequest).Name;
        logger.LogDebug("Validating {RequestName}", requestName);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for {RequestName} with {ErrorCount} error(s): {Errors}",
                requestName,
                validationResult.Errors.Count,
                string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            return Result.Fail(validationResult.ToValidationErrors());
        }

        logger.LogDebug("Validation passed for {RequestName}", requestName);
        return await next();
    }
}