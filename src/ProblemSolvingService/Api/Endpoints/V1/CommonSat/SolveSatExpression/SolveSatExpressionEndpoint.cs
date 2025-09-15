using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatExpression;

public static class SolveSatExpressionEndpoint
{
    public static async Task<Results<Ok<SolveSatExpressionResponse>, ValidationProblem>> Execute(
        [FromBody] SolveSatExpressionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        Result<SolveSatExpressionCommandResult> result =
            await sender.Send(request.ToSolveSatExpressionCommand(), cancellationToken);

        if (!result.IsFailed)
            return TypedResults.Ok(SolveSatExpressionResponse.From(result.Value));

        Dictionary<string, string[]> errors = [];
        string message = result.Errors.OfType<SatParseError>().First().Message;
        errors.Add(nameof(request.SatExpression), [message]);
        return TypedResults.ValidationProblem(errors);
    }
}