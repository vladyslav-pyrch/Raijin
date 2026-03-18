namespace Raijin.SatSolver.Application.Messaging;

public sealed class MessageProcessingException(string error) : Exception(error);