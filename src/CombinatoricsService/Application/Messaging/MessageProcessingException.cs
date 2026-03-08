namespace Raijin.CombinatoricsService.Application.Messaging;

public class MessageProcessingException(string error) : Exception(error);