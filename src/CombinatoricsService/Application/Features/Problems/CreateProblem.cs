using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class CreateProblemHandler(
    IEnumerable<IInstanceFactory> problemInstanceFactories,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<CreateProblemCommand, CreateProblemResult>
{
    public async Task<Result<CreateProblemResult>> Handle(CreateProblemCommand request,
        CancellationToken cancellationToken)
    {
        IInstanceFactory? instanceFactory = problemInstanceFactories
            .FirstOrDefault(factory => factory.ProblemType == request.ProblemType);

        if (instanceFactory is null)
            throw new InvalidOperationException(
                $"No instance factory found for problem type {request.ProblemType}");

        Result<Instance> instanceResult = instanceFactory.CreateInstance(request.Instance);
        if (instanceResult.IsFailed)
            return instanceResult.MapErrors(error => error switch
            {
                ValidationError validationError => new ValidationError(
                    validationError.PropertyName,
                    validationError.Problem
                ),
                _ => error
            }).ToResult<CreateProblemResult>();

        var id = Guid.CreateVersion7();
        var problem = Problem.Create(id, request.Name, request.Description, request.ProblemType);
        problem.SetInstance(instanceResult.Value);

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateProblemResult(id);
    }
}

public sealed record CreateProblemCommand(
    string Name,
    string Description,
    string ProblemType,
    InstanceDto Instance
) : IRequest<CreateProblemResult>;

public sealed record CreateProblemResult(
    Guid Id
);

public sealed class CreateProblemValidator : AbstractValidator<CreateProblemCommand>
{
    public CreateProblemValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(command => command.Description)
            .NotNull()
            .MaximumLength(5000);
        RuleFor(command => command.ProblemType)
            .NotEmpty()
            .MaximumLength(100);
    }
}