using System.Text.Json.Serialization;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public interface ISetProblemInstanceExtension
{
    public string ProblemType { get; }

    public Result<Instance> CreateInstance(InstanceDto instanceDto);
}

public sealed class SetProblemInstanceCommandHandler(
    IEnumerable<ISetProblemInstanceExtension> problemInstanceFactories,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetProblemInstanceCommand>
{
    public async Task<Result> Handle(SetProblemInstanceCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus == SolvingStatus.Running)
            return new ConflictError("Cannot change instance while solving is in progress.");

        ISetProblemInstanceExtension? instanceFactory = problemInstanceFactories
            .FirstOrDefault(factory => factory.ProblemType == request.Instance.ProblemType);

        if (instanceFactory is null)
            throw new InvalidOperationException(
                $"Unsupported problem type: {request.Instance.ProblemType}");

        Result<Instance> instanceResult = instanceFactory.CreateInstance(request.Instance);

        if (instanceResult.IsFailed)
            return instanceResult.MapErrors(error => error switch
            {
                ValidationError validationError => new ValidationError(
                    $"{nameof(request.Instance)}.{validationError.PropertyName}",
                    validationError.Problem
                ),
                _ => error
            }).ToResult();

        problem.SetInstance(instanceResult.Value);

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record SetProblemInstanceCommand(
    Guid ProblemId,
    InstanceDto Instance
) : IRequest;

[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstanceDto), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemInstanceDto), ProblemTypes.BooleanProblem)]
[JsonDerivedType(typeof(CspInstanceDto), ProblemTypes.ConstraintSatisfiabilityProblem)]
public abstract record InstanceDto
{
    public abstract string ProblemType { get; }
}

public sealed class SetProblemInstanceValidator : AbstractValidator<SetProblemInstanceCommand>
{
    public SetProblemInstanceValidator(IServiceProvider serviceProvider)
    {
        RuleFor(command => command.ProblemId)
            .NotEmpty();
        RuleFor(command => command.Instance)
            .NotNull()
            .SetValidator((_, dto) =>
                serviceProvider.GetRequiredService(typeof(IValidator<>).MakeGenericType(dto.GetType())) as
                    IValidator<InstanceDto>
            );
    }
}