namespace Raijin.Application.Contracts;

public interface IMessage
{
    public Guid CorrelationId { get; }
}