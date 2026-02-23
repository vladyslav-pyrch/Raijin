using FluentResults;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public class DimacsFormatValidationError(string error) : Error(error);