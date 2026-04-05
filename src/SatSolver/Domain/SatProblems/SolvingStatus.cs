namespace Raijin.SatSolver.Domain.SatProblems;

public enum SolvingStatus
{
    Pending,
    Solving,
    Solved,
    Failed,
    TimeOut
}