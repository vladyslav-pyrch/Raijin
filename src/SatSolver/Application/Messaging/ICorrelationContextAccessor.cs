namespace Raijin.SatSolver.Application.Messaging;

public interface ICorrelationContextAccessor
{
    public CorrelationContext CorrelationContext { get; set; }
}