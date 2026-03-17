namespace Raijin.Application.Contracts;

public interface IMessage
{
    public string MessageId { get; }
    
    public string CorrelationId { get; }
    
    public string CausationId { get; }
    
    public DateTime Timestamp { get; }
}