using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;

public sealed record GetCspInstanceQuery(Guid ProblemId) : IRequest<GetCspInstanceResult>;

public sealed class GetCspInstanceHandler(
    IProblemRepository problemRepository,
    ILogger<GetCspInstanceHandler> logger
) : IRequestHandler<GetCspInstanceQuery, GetCspInstanceResult>
{
    public async Task<Result<GetCspInstanceResult>> Handle(
        GetCspInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (problem.Instance is not CspInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a CSP instance.");

        IReadOnlyList<DecisionVariableDto> variables = instance.Variables
            .Select(v => new DecisionVariableDto(v.Name, v.States))
            .ToList();

        IReadOnlyList<string> constraints = instance.Constraints
            .Select(c => c.ToString())
            .ToList();

        logger.LogDebug(
            "CSP instance read. ProblemId={ProblemId} VariableCount={VariableCount} ConstraintCount={ConstraintCount}",
            request.ProblemId,
            variables.Count,
            constraints.Count);

        return new GetCspInstanceResult(new CspInstanceDto(variables, constraints));
    }
}

public sealed record GetCspInstanceResult(CspInstanceDto Instance);

public sealed class GetCspInstanceValidator : AbstractValidator<GetCspInstanceQuery>
{
    public GetCspInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty().WithMessage("Problem identifier is required.");
    }
}
