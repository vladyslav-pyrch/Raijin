using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

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
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SatEncoding is null)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a SAT encoding.");

        IReadOnlyList<IReadOnlyList<int>> clauses = problem.SatEncoding.Clauses
            .Select(clause => (IReadOnlyList<int>)clause.ToList())
            .ToList();

        return new GetSatEncodingResult(
            problem.SatEncoding.NumberOfVariables,
            problem.SatEncoding.NumberOfClauses,
            clauses);
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
