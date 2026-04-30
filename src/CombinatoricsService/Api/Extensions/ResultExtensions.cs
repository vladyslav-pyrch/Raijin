using FluentResults;
using Raijin.CombinatoricsService.Application.Errors;

namespace Raijin.CombinatoricsService.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemResult(this ResultBase result)
    {
        var errorsWithStatus = result.Errors
            .Select(e => (error: e, status: GetHttpStatus(e)))
            .ToList();

        var (primaryError, statusCode) = errorsWithStatus.MaxBy(x => x.status);

        return Results.Problem(
            detail: primaryError.Message,
            statusCode: statusCode,
            title: GetTitle(statusCode),
            type: GetType(statusCode),
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = result.Errors.Select(e => e.Message).ToArray()
            });
    }

    private static int GetHttpStatus(IError error) => error switch
    {
        ValidationError => StatusCodes.Status400BadRequest,
        NotFoundError => StatusCodes.Status404NotFound,
        ConflictError => StatusCodes.Status409Conflict,
        DomainError => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        _ => "Bad Request"
    };

    private static string GetType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc9110#section-15.5.21",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.5.1"
    };
}
