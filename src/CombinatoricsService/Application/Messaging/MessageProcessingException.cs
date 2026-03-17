namespace Raijin.CombinatoricsService.Application.Messaging;

public sealed class MessageProcessingException(string error) : Exception(error);