using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public sealed record SubmitBooleanProblemCommand(
    string BooleanFormula,
    Guid? BooleanProblemId = null)
    : IRequest<SubmitBooleanProblemResult>;