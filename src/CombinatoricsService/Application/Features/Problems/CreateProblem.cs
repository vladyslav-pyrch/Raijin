using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class CreateProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateProblemCommand, CreateProblemResult>
{
    public async Task<Result<CreateProblemResult>> Handle(CreateProblemCommand request,
        CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        var problem = Problem.Create(id, request.Name, request.Description);

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateProblemResult(id);
    }
}

public sealed record CreateProblemCommand(
    string Name,
    string Description
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
    }
}