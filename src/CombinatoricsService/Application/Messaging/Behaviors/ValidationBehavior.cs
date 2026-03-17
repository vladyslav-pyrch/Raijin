using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Raijin.CombinatoricsService.Application.Validation;

namespace Raijin.CombinatoricsService.Application.Messaging.Behaviors;

public class ValidationBehavior<TRequest, TResult>(IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<Result<TResult>> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result<TResult>>> next)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.ToValidationErrors());
        
        return await next();
    }
}

public class ValidationBehavior<TRequest>(IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public async Task<Result> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<Result>> next)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.ToValidationErrors());
        
        return await next();
    }
}