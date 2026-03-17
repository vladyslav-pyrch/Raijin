namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IMessageContextAccessor
{
    public MessageContext CurrentContext { get; set; }
}