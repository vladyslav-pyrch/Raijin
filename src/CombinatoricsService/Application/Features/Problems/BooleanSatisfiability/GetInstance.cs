using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public sealed class GetBooleanSatisfiabilityInstanceHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetBooleanSatisfiabilityInstanceQuery, GetBooleanSatisfiabilityInstanceResult>
{
    public async Task<Result<GetBooleanSatisfiabilityInstanceResult>> Handle(
        GetBooleanSatisfiabilityInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is not BooleanSatisfiabilityInstance instance)
            return new NotFoundError(
                $"Problem '{request.ProblemId}' does not have a boolean satisfiability instance.");

        IReadOnlyList<IReadOnlyList<string>> clauses = instance.Clauses
            .Select(clause => (IReadOnlyList<string>)clause.Literals
                .Select(l => l.Negated ? $"~{l.Variable.Name}" : l.Variable.Name)
                .ToList())
            .ToList();

        return new GetBooleanSatisfiabilityInstanceResult(new SatInstanceDto(clauses));
    }
}

public sealed record GetBooleanSatisfiabilityInstanceQuery(Guid ProblemId)
    : IRequest<GetBooleanSatisfiabilityInstanceResult>;

public sealed record GetBooleanSatisfiabilityInstanceResult(SatInstanceDto Instance );

public sealed class GetBooleanSatisfiabilityInstanceValidator
    : AbstractValidator<GetBooleanSatisfiabilityInstanceQuery>
{
    public GetBooleanSatisfiabilityInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
