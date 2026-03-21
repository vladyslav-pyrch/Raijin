namespace Raijin.CombinatoricsService.Application.Messaging;

public interface ICorrelationContextAccessor
{
    public CorrelationContext CorrelationContext { get; set; }
}