using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.EdgeColoring;

public sealed class CreateEdgeColoringFromDimacsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/edge-coloring-dimacs", Execute)
            .WithName("create edge coloring problem from dimacs")
            .WithTags("edge-coloring")
            .Accepts<CreateEdgeColoringFromDimacsRequest>("multipart/form-data")
            .Produces<CreateEdgeColoringFromDimacsResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();
    }

    public static async Task<IResult> Execute(
        [FromForm] CreateEdgeColoringFromDimacsRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<string> dimacsResult = await ReadDimacs(request.File, ".col", cancellationToken);

        if (dimacsResult.IsFailed)
            return dimacsResult.ToProblemResult();

        Result<CreateEdgeColoringFromDimacsResult> result = await mediator.Send(
            new CreateEdgeColoringFromDimacsCommand(
                new ProblemDetailsDto(
                    request.Name,
                    request.Description ?? string.Empty
                ),
                dimacsResult.Value,
                request.ColorCount
            ),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateEdgeColoringFromDimacsResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }

    private static async Task<Result<string>> ReadDimacs(
        IFormFile? file,
        string expectedExtension,
        CancellationToken cancellationToken)
    {
        if (file is null)
            return Result.Fail(new ValidationError(nameof(CreateEdgeColoringFromDimacsRequest.File), "DIMACS file is required."));

        if (!string.Equals(Path.GetExtension(file.FileName), expectedExtension, StringComparison.OrdinalIgnoreCase))
            return Result.Fail(new ValidationError(nameof(CreateEdgeColoringFromDimacsRequest.File), $"DIMACS file must have {expectedExtension} extension."));

        await using Stream stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}

public sealed class CreateEdgeColoringFromDimacsRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int ColorCount { get; set; }

    public IFormFile File { get; set; } = null!;
}

public sealed record CreateEdgeColoringFromDimacsResponse(Guid ProblemId);
