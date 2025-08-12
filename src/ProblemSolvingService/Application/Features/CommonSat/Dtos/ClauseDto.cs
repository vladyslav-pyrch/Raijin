using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

public record ClauseDto(List<LiteralDto> Literals)
{
    public Clause ToClause() => new(Literals.Select(literalDto => literalDto.ToLiteral()).ToList());
}