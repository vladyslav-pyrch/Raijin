using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public record SubmitBooleanProblemCommand(
    string BooleanFormula,
    MessageContext Context,
    Guid? BooleanProblemId = null)
    : IRequest<SubmitBooleanProblemResult>, IContextualRequest;