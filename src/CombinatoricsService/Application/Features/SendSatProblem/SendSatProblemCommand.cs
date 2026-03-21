using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed record SendSatProblemCommand(Guid SatProblemId, string Dimacs)
    : IRequest<SendSatProblemResult>;