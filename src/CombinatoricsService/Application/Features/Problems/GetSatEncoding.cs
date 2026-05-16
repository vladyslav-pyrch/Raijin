using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record GetSatEncodingQuery(Guid ProblemId) : IRequest<GetSatEncodingResult>;

public sealed class GetSatEncodingHandler(
    IProblemRepository problemRepository,
    ILogger<GetSatEncodingHandler> logger
) : IRequestHandler<GetSatEncodingQuery, GetSatEncodingResult>
{
    public async Task<Result<GetSatEncodingResult>> Handle(
        GetSatEncodingQuery request,
        CancellationToken cancellationToken)
    {
        GetSatEncodingResult? result = await problemRepository.GetSatEncodingByProblemId(request.ProblemId, cancellationToken);

        if (result is null)
            return new NotFoundError($"There is no problem with id '{request.ProblemId}' or the problem does not have a SAT encoding.");

        logger.LogDebug(
            "SAT encoding read. ProblemId={ProblemId} VariableCount={VariableCount} ClauseCount={ClauseCount}",
            request.ProblemId,
            result.NumberOfVariables,
            result.NumberOfClauses);

        return result;
    }
}

public sealed record GetSatEncodingResult(
    int NumberOfVariables,
    int NumberOfClauses,
    IReadOnlyList<IReadOnlyList<int>> Clauses
);

public sealed class GetSatEncodingValidator : AbstractValidator<GetSatEncodingQuery>
{
    public GetSatEncodingValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
