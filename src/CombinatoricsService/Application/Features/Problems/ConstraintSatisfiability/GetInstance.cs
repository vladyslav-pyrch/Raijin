using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed record GetCspInstanceQuery(Guid ProblemId) : IRequest<GetCspInstanceResult>;

public sealed class GetCspInstanceHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetCspInstanceQuery, GetCspInstanceResult>
{
    public async Task<Result<GetCspInstanceResult>> Handle(
        GetCspInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is not CspInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a CSP instance.");

        IReadOnlyList<CspVariableResult> variables = instance.Variables
            .Select(v => new CspVariableResult(v.Name, v.States))
            .ToList();

        IReadOnlyList<string> constraints = instance.Constraints
            .Select(c => c.ToString())
            .ToList();

        return new GetCspInstanceResult(variables, constraints);
    }
}

public sealed record GetCspInstanceResult(
    IReadOnlyList<CspVariableResult> Variables,
    IReadOnlyList<string> Constraints
);

public sealed record CspVariableResult(string Name, IReadOnlyList<string> States);

public sealed class GetCspInstanceValidator : AbstractValidator<GetCspInstanceQuery>
{
    public GetCspInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
