using FluentResults;
using FluentValidation.Results;
using Raijin.SatSolver.Application.Errors;

namespace Raijin.SatSolver.Application.Validation;

public static class ValidationResultExtensions
{
    public static IEnumerable<Error> ToValidationErrors(this ValidationResult validationResult) =>
        validationResult.Errors.Select(error =>
            new ValidationError(error.PropertyName, error.ErrorMessage)
                .WithMetadata("ErrorCode", error.ErrorCode)
        );
}