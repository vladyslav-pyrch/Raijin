using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record GetSatEncodingQuery(Guid ProblemId) : IRequest<GetSatEncodingResult>;

public sealed class GetSatEncodingHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetSatEncodingQuery, GetSatEncodingResult>
{
    public async Task<Result<GetSatEncodingResult>> Handle(
        GetSatEncodingQuery request,
        CancellationToken cancellationToken)
    {
        GetSatEncodingResult? result = await problemRepository.GetSatEncodingByProblemId(request.ProblemId, cancellationToken);

        if (result is null)
            return new NotFoundError($"There is no problem with id '{request.ProblemId}' or the problem does not have a SAT encoding.");

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
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
