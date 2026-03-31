using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class UpdateProblemHandler(
    IEnumerable<IInstanceFactory> InstanceFactories,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProblemCommand>
{
    public async Task<Result> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.Id, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.Id);

        if (request.Name is not null)
            problem.UpdateName(request.Name);

        if (request.Description is not null)
            problem.UpdateDescription(request.Description);

        if (request.Instance is not null)
        {
            IInstanceFactory? instanceFactory = InstanceFactories
                .FirstOrDefault(factory => factory.ProblemType == request.ProblemType);

            if (instanceFactory is null)
                throw new InvalidOperationException(
                    $"No factory found for problem kind {request.ProblemType!}");

            Result<Instance> instanceResult = instanceFactory.CreateInstance(request.Instance);
            if (instanceResult.IsFailed)
                return instanceResult.MapErrors(error => error switch
                {
                    ValidationError validationError => new ValidationError(
                        validationError.PropertyName,
                        validationError.Problem
                    ),
                    _ => error
                }).ToResult();

            problem.SetInstance(instanceResult.Value);
        }

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record UpdateProblemCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? ProblemType,
    InstanceDto? Instance
) : IRequest;

public sealed class UpdateProblemValidator : AbstractValidator<UpdateProblemCommand>
{
    public UpdateProblemValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100);
        RuleFor(x => x.Description)
            .MaximumLength(5000);
        RuleFor(command => command)
            .Custom((command, context) =>
            {
                if (command.Instance is not null && string.IsNullOrEmpty(command.ProblemType))
                    context.AddFailure(new ValidationFailure(nameof(command.ProblemType),
                        "ProblemType must be provided when Instance is provided."));
            });
    }
}