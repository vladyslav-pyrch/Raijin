using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public class CreateProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<CreateProblemCommand, CreateProblemResult>
{
    public async Task<Result<CreateProblemResult>> Handle(CreateProblemCommand request,
        CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        var problem = Problem.Create(id, request.Name, request.Description, request.ProblemKind);

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateProblemResult(id);
    }
}

public record CreateProblemCommand(
    string Name,
    string Description,
    string ProblemKind
) : IRequest<CreateProblemResult>;

public record CreateProblemResult(
    Guid Id
);

public class CreateProblemValidator : AbstractValidator<CreateProblemCommand>
{
    public CreateProblemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);
        RuleFor(x => x.ProblemKind)
            .NotEmpty()
            .MaximumLength(100);
    }
}