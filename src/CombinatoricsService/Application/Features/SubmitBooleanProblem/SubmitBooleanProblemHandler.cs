using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanProblems;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public sealed class SubmitBooleanProblemHandler(
    IEventStore eventStore,
    IMessageBus messageBus,
    ILogger<SubmitBooleanProblemHandler> logger
) : IRequestHandler<SubmitBooleanProblemCommand, SubmitBooleanProblemResult>
{
    public async Task<Result<SubmitBooleanProblemResult>> Handle(SubmitBooleanProblemCommand request,
        CancellationToken cancellationToken)
    {
        Guid booleanProblemId = request.BooleanProblemId ?? Guid.CreateVersion7();

        var booleanProblem = BooleanProblem.Create(booleanProblemId, request.BooleanFormula);
        string dimacs = booleanProblem.ReduceToSat().Dimacs;

        await eventStore.Save(booleanProblem, cancellationToken);

        await messageBus.Publish<IBooleanProblemSubmitted>(new
        {
            BooleanProblemId = booleanProblem.Id,
            BooleanFormula = booleanProblem.Formula
        }, cancellationToken);
        await messageBus.Publish<ISatProblemSent>(new
        {
            SatProblemId = booleanProblem.Id,
            Dimacs = dimacs
        }, cancellationToken);

        return new SubmitBooleanProblemResult(booleanProblemId);
    }
}