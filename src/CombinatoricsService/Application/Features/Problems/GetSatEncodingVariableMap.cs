using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record GetSatEncodingVariableMapQuery(Guid ProblemId) : IRequest<GetSatEncodingVariableMapResult>;

public sealed class GetSatEncodingVariableMapHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetSatEncodingVariableMapQuery, GetSatEncodingVariableMapResult>
{
    public async Task<Result<GetSatEncodingVariableMapResult>> Handle(
        GetSatEncodingVariableMapQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SatEncoding is null)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a SAT encoding.");

        IReadOnlyDictionary<string, int>? variableMap = problem.ComputeVariableMap();

        if (variableMap is null)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have an instance.");

        IReadOnlyList<VariableMapEntry> variables = variableMap
            .Select(kvp => new VariableMapEntry(kvp.Key, kvp.Value))
            .OrderBy(e => e.Index)
            .ToList();

        return new GetSatEncodingVariableMapResult(variables);
    }
}

public sealed record GetSatEncodingVariableMapResult(IReadOnlyList<VariableMapEntry> Variables);

public sealed record VariableMapEntry(string Name, int Index);

public sealed class GetSatEncodingVariableMapValidator : AbstractValidator<GetSatEncodingVariableMapQuery>
{
    public GetSatEncodingVariableMapValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
