using System.Text.Json.Serialization;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class SetProblemInstanceCommandHandler(
    IEnumerable<IInstanceFactory> problemInstanceFactories,
    IServiceProvider serviceProvider,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<SetProblemInstanceCommand>
{
    public async Task<Result> Handle(SetProblemInstanceCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        IInstanceFactory? instanceFactory = problemInstanceFactories
            .FirstOrDefault(factory => factory.ProblemType == request.Instance.ProblemType);

        if (instanceFactory is null)
            throw new InvalidOperationException(
                $"No instance factory found for problem type {request.Instance.ProblemType}");

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