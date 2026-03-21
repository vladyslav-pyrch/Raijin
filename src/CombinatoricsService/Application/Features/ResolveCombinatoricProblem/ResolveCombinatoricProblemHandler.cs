//using FluentResults;
//using Microsoft.Extensions.Logging;
//using Raijin.Application.Contracts;
//using Raijin.CombinatoricsService.Application.Errors;
//using Raijin.CombinatoricsService.Application.Messaging;
//using Raijin.CombinatoricsService.Application.Persistence;
//using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
//using Raijin.CombinatoricsService.Domain.Logic;
//using Raijin.CombinatoricsService.Domain.Shared;

//namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

//public sealed class ResolveCombinatoricProblemHandler(
//    ICombinatoricProblemRepository combinatoricProblemRepository,
//    IUnitOfWork unitOfWork,
//    IMessageBus messageBus,
//    IMessageIdGenerator messageIdGenerator,
//    IMessageContextAccessor messageContextAccessor,
//    ILogger<ResolveCombinatoricProblemHandler> logger
//) : IRequestHandler<ResolveCombinatoricProblemCommand>
//{
//    public async Task<Result> Handle(
//        ResolveCombinatoricProblemCommand request,
//        CancellationToken cancellationToken)
//    {
//        logger.LogInformation("Resolving SAT solution for combinatoric problem {CombinatoricProblemId}",
//            request.CombinatoricProblemId);

//        CombinatoricProblem? combinatoricProblem = await combinatoricProblemRepository.GetById(
//            request.CombinatoricProblemId, cancellationToken);

//        if (combinatoricProblem is null)
//        {
//            logger.LogInformation(
//                "No combinatoric problem found for SAT problem {CombinatoricProblemId}, skipping resolution",
//                request.CombinatoricProblemId);
//            return Result.Fail(new NotFoundError(nameof(CombinatoricProblem), request.CombinatoricProblemId));
//        }

//        //Redo to make it more DDD like
//        var satSolution = new SatSolution(request.SatSolution);
//        BooleanProblem booleanProblem = combinatoricProblem.ReduceToBooleanProblem();
//        booleanProblem.ResolveSatSolution(satSolution);
//        combinatoricProblem.ResolveSatSolution(booleanProblem.Solution ?? new BooleanProblemSolution([]));

//        logger.LogInformation(
//            "Combinatoric problem {CombinatoricProblemId} resolved with satisfiability {Satisfiability}",
//            request.CombinatoricProblemId, combinatoricProblem.Satisfiability);

//        await combinatoricProblemRepository.Update(combinatoricProblem, cancellationToken);
//        await unitOfWork.Commit(cancellationToken);

//        Dictionary<string, string> solutionDictionary = combinatoricProblem.Solution?.Assignments
//            .ToDictionary(a => a.DecisionVariable.Name, a => a.SelectedState) ?? [];

//        await messageBus.Publish<ICombinatoricProblemSolved>(new
//        {
//            MessageId = messageIdGenerator.NextMessageId(),
//            messageContextAccessor.CurrentContext.CorrelationId,
//            messageContextAccessor.CurrentContext.CausationId,
//            Timestamp = DateTimeOffset.UtcNow,
//            CombinatoricProblemId = request.CombinatoricProblemId.ToString(),
//            Solution = solutionDictionary
//        }, cancellationToken);

//        logger.LogInformation("Combinatoric problem {CombinatoricProblemId} solution published successfully",
//            request.CombinatoricProblemId);

//        return Result.Ok();
//    }
//}

