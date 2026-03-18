namespace Raijin.SatSolver.Application.Messaging;

public interface IMessageContextAccessor
{
    public MessageContext CurrentContext { get; set; }
}