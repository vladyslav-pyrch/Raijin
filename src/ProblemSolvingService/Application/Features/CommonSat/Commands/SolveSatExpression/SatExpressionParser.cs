using FluentResults;
using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public class SatExpressionParser()
{
    public Result<IDictionary<string, SatVariable>> ParseInto(string satExpression, SatProblem satProblem)
    {
        List<SatToken> tokens = SatExpressionTokenizer.Tokenize(satExpression).ToList();
        var symbolTable = new Dictionary<string, SatVariable>();
        var varId = 1;

        if (tokens.Any(IsUnknown))
        {
            SatToken unknownToken = tokens.First(IsUnknown);
            return new SatParseError($"Unknown token '{unknownToken.Value}'", unknownToken.Index);
        }

        var i = -1;
        while (CanTakeTokens())
        {
            if (TakeToken() is { Type: not SatTokenType.LeftBracket} notLeftBracket)
                return new SatParseError("Expected '('", notLeftBracket.Index);

            var literals = new List<Literal>();
            while (CanTakeTokens() && TakeToken() is { Type: SatTokenType.Literal } literalToken)
            {
                string variableName = GetNameOf(literalToken);
                bool negated = IsNegate(literalToken);
                SatVariable satVariable = GetSatVariableOf(variableName);
                literals.Add(negated ? Literal.Negated(satVariable) : Literal.Affirmed(satVariable));
            }

            if (!ThereAreTokens())
                return new SatParseError("Expected ')'", satExpression.Length - 1);

            if (CurrentToken() is { Type: not SatTokenType.RightBracket} notRightBracket)
                return new SatParseError("Expected ')'", notRightBracket.Index);

            if (literals.Count == 0)
                return new SatParseError("No empty clauses are allowed.", PreviousToken().Index);

            satProblem.AddClause(literals);
        }

        return Result.Ok();

        SatToken PreviousToken() => tokens[i - 1];
        SatToken CurrentToken() => tokens[i];
        SatToken TakeToken() => tokens[++i];
        bool ThereAreTokens() => i < tokens.Count;
        bool CanTakeTokens() => i + 1 < tokens.Count;
        bool IsUnknown(SatToken token) => token is { Type: SatTokenType.Unknown };

        bool IsNegate(SatToken token) => token.Value.StartsWith('~');
        string GetNameOf(SatToken token) => token.Value.Trim('~');
        SatVariable NextSatVariable() => new(varId++);
        SatVariable GetSatVariableOf(string name)
        {
            if (!symbolTable.TryGetValue(name, out SatVariable? satVariable))
                symbolTable[name] = satVariable = NextSatVariable();

            return satVariable;
        }
    }
}