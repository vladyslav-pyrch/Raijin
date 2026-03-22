using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public sealed class SubmitBooleanProblemHandler(
    IBooleanProblemRepository booleanProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<SubmitBooleanProblemHandler> logger
) : IRequestHandler<SubmitBooleanProblemCommand, SubmitBooleanProblemResult>
{
    public async Task<Result<SubmitBooleanProblemResult>> Handle(SubmitBooleanProblemCommand request,
        CancellationToken cancellationToken)
    {
        Guid booleanProblemId = request.BooleanProblemId ?? Guid.CreateVersion7();
        BooleanProblem booleanProblem = new(booleanProblemId);

        Result result = Result.Try(
            () => booleanProblem.SetExpression(request.BooleanFormula),
            exception => new ValidationError(nameof(request.BooleanFormula), exception.Message)
        );

        if (result.IsFailed)
            return result;

        await booleanProblemRepository.Add(booleanProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<IBooleanProblemSubmitted>(new
        {
            BooleanProblemId = booleanProblem.Id,
            BooleanFormula = booleanProblem.Formula
        }, cancellationToken);

        return new SubmitBooleanProblemResult(booleanProblemId);
    }
}