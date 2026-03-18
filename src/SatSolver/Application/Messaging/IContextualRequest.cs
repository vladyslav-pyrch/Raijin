namespace Raijin.SatSolver.Application.Messaging;

public interface IContextualRequest
{
    public MessageContext Context { get; }
}