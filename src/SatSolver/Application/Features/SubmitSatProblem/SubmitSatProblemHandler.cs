using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Cqrs;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Validation;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public class SubmitSatProblemHandler(
    IValidator<SubmitSatProblemCommand> validator,
    IMessageBus messageBus,
    ILogger<SubmitSatProblemHandler> logger
) : IRequestHandler<SubmitSatProblemCommand, Result<SubmitSatProblemResult>>
{
    public async Task<Result<SubmitSatProblemResult>> Handle(SubmitSatProblemCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.ToValidationErrors());

        var satProblemId = Guid.CreateVersion7();

        await messageBus.Publish<ISatProblemSubmitted>(message: new
        {
            SatProblemId = satProblemId,
            Dimacs = command.Dimacs,
        }, cancellationToken);

        return new SubmitSatProblemResult(satProblemId);
    }
}