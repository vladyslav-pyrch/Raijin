using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

public record LiteralDto(int VariableNumber, bool IsNegated)
{
    public Literal ToLiteral() => new(new SatVariable(VariableNumber), IsNegated);
}