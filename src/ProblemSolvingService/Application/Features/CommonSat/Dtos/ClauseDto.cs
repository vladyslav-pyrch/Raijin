using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

public record ClauseDto(List<LiteralDto> Literals)
{
    public List<Literal> ToListOfLiterals() => Literals.Select(literalDto => literalDto.ToLiteral()).ToList();
}