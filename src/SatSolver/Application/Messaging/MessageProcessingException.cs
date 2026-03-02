namespace Raijin.SatSolver.Application.Messaging;

public class MessageProcessingException(string error) : Exception(error);