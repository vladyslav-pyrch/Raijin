using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SubmitSatProblem;

public sealed record SubmitSatProblemCommand(Guid CombinatoricProblemId, MessageContext Context)
    : IRequest<SubmitSatProblemResult>, IContextualRequest;

