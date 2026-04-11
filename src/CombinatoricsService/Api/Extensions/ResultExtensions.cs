using System.Diagnostics.CodeAnalysis;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Application.Errors;

namespace Raijin.CombinatoricsService.Api.Extensions;

public static class ResultExtensions
{
    public static NotFound<ProblemDetails> ToNotFoundResult(this NotFoundError notFoundError)
        => TypedResults.NotFound(new ProblemDetails
        {
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = notFoundError.Message,
            Extensions = notFoundError.Metadata
        });

    public static Conflict<ProblemDetails> ToConflictResult(this ConflictError conflictError)
        => TypedResults.Conflict(new ProblemDetails
        {
            Title = "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = conflictError.Message,
            Extensions = conflictError.Metadata
        });

    public static UnprocessableEntity<ProblemDetails> ToUnprocessableEntityResult(this DomainError domainError)
        => TypedResults.UnprocessableEntity(new ProblemDetails
        {
            Title = "Unprocessable Entity",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = domainError.Message
        });

    public static ValidationProblem ToValidationProblemResult(this IReadOnlyList<ValidationError> validationErrors)
        => TypedResults.ValidationProblem(
            validationErrors.GroupBy(error => error.PropertyName).ToDictionary(
                grouping => grouping.Key,
                grouping => grouping.Select(error => error.Message).ToArray()
            ),
            title: "Validation Problem",
            detail: "One or more validation errors has occured",
            extensions: validationErrors.GroupedMetadata()!
        );

    private static Dictionary<string, object> GroupedMetadata(this IReadOnlyList<IError> errors)
    {
        Dictionary<string, List<object>> groupedMetadata = [];

        foreach (IError error in errors)
        foreach (KeyValuePair<string, object> kvp in error.Metadata)
        {
            if (!groupedMetadata.TryGetValue(kvp.Key, out List<object>? values))
            {
                values = [];
                groupedMetadata[kvp.Key] = values;
            }

            values.Add(kvp.Value);
        }

        return groupedMetadata.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count == 1 ? kvp.Value.First() : kvp.Value.ToArray()
        );
    }

    extension(ResultBase result)
    {
        public IError? Error => result.Errors.FirstOrDefault();

        public bool Has<TError>([NotNullWhen(true)] out TError? error) where TError : IError
        {
            error = result.Errors.OfType<TError>().FirstOrDefault();

            return error != null;
        }

        public bool Has<TError>([NotNullWhen(true)] out IReadOnlyList<TError>? errors) where TError : IError
        {
            errors = result.Errors.OfType<TError>().ToList();

            return errors.Count > 0;
        }
    }
}