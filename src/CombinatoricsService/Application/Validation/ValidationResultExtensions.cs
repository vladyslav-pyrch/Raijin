using FluentValidation.Results;
using Raijin.CombinatoricsService.Application.Errors;

namespace Raijin.CombinatoricsService.Application.Validation;

public static class ValidationResultExtensions
{
    public static IEnumerable<ValidationError> ToValidationErrors(this ValidationResult validationResult) =>
        validationResult.Errors.Select(error =>
            new ValidationError(error.PropertyName, error.ErrorMessage)
        );
}