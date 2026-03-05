using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public record TseitinTransformResult(SatProblem Problem, IReadOnlyBijectiveDictionary<Variable, int> SymbolTable);