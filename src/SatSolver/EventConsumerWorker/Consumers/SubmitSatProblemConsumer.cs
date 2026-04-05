using FluentResults;
using MassTransit;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Features.SatProblems;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Infrastructure.Messaging;

namespace Raijin.SatSolver.EventConsumerWorker.Consumers;

public sealed class SubmitSatProblemConsumer(
    IMediator mediator
) : IConsumer<ISubmitSatProblem>
{
    public async Task Consume(ConsumeContext<ISubmitSatProblem> context)
    {
        Result result = await mediator.Send(new CreateSatProblemCommand(
            context.Message.ProblemId,
            context.Message.Clauses
        ), context.CancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(
                string.Join("; ", result.Errors.Select(error => error.Message))
            );
    }
}