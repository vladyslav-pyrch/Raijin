namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class MessageProcessingException(string error) : Exception(error);