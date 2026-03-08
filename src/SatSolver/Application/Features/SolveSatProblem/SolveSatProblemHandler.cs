using FluentResults;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Application.Validation;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public class SolveSatProblemHandler(
    ISatProblemRepository repository,
    ISatSolver solver,
    SolveSatProblemValidator validator,
    IMessageBus messageBus,
    ILogger<SolveSatProblemHandler> logger
) : IRequestHandler<SolveSatProblemCommand, Result>
{
    public async Task<Result> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.ToValidationErrors());
        
        var satProblem = SatProblem.Create(command.SatProblemId, command.Dimacs);
        
        await repository.Add(satProblem, cancellationToken);
        await repository.Save(cancellationToken);

        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        await repository.Update(satProblem, cancellationToken);
        await repository.Save(cancellationToken);
        
        await messageBus.Publish<ISatProblemSolved>(new
        {
            SatProblemId = satProblem.Id,
            Solution = solution
        }, cancellationToken);

        return Result.Ok();
    }
}