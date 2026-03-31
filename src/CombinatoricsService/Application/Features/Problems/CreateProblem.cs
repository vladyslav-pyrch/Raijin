using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class CreateProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<CreateProblemCommand, CreateProblemResult>
{
    public async Task<Result<CreateProblemResult>> Handle(CreateProblemCommand request,
        CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        var problem = Problem.Create(id, request.Name, request.Description, request.ProblemType);

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateProblemResult(id);
    }
}

public sealed record CreateProblemCommand(
    string Name,
    string Description,
    string ProblemType
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
            .MaximumLength(100)
            .Must(ProblemTypes.IsValid)
            .WithMessage($"The valid problem types are: {string.Join(", ", ProblemTypes.GetAll())}");
    }
}