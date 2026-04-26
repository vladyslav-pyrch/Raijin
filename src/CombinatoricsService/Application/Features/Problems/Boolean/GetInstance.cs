using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public sealed class GetBooleanInstanceHandler(
    IProblemRepository problemRepository
) : IRequestHandler<GetBooleanInstanceQuery, GetBooleanInstanceResult>
{
    public async Task<Result<GetBooleanInstanceResult>> Handle(
        GetBooleanInstanceQuery request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.Instance is not BooleanProblemInstance instance)
            return new NotFoundError($"Problem '{request.ProblemId}' does not have a boolean instance.");

        return new GetBooleanInstanceResult(
            new BooleanProblemInstanceDto(instance.Root.ToString())
        );
    }
}

public sealed record GetBooleanInstanceQuery(Guid ProblemId) : IRequest<GetBooleanInstanceResult>;

public sealed record GetBooleanInstanceResult(BooleanProblemInstanceDto Instance);

public sealed class GetBooleanInstanceValidator : AbstractValidator<GetBooleanInstanceQuery>
{
    public GetBooleanInstanceValidator()
    {
        RuleFor(q => q.ProblemId).NotEmpty();
    }
}
