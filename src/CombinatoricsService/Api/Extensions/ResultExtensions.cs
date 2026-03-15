using FluentResults;
using Raijin.CombinatoricsService.Application.Errors;

namespace Raijin.CombinatoricsService.Api.Extensions;

public static class ResultExtensions
{
    public static IDictionary<string, string[]> ToValidationErrorDictionary(this Result result)
    {
        return result.Errors.OfType<ValidationError>()
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Problem).ToArray()
            );
    }

    public static IDictionary<string, string[]> ToValidationErrorDictionary<T>(this Result<T> result) =>
        result.ToResult().ToValidationErrorDictionary();
}