using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed record SendSatProblemCommand(Guid CombinatoricProblemId, MessageContext Context)
    : IRequest<SendSatProblemResult>, IContextualRequest;

