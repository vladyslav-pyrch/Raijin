using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Raijin.SatSolver.Application.Validation;

namespace Raijin.SatSolver.Application.Messaging.Behaviors;

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