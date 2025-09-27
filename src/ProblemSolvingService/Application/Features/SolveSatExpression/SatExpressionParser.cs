using System.Diagnostics.CodeAnalysis;
using FluentResults;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public sealed class SatExpressionParser
{
    private int _varId = 1;

    private readonly BijectiveDictionary<string, int> _symbolTable = [];

    public BijectiveDictionary<string, int> SymbolTable => _symbolTable;

    public Result<List<ClauseDto>> ParseClauses(string satExpression)
    {
        var tokens = new SatTokens(SatExpressionTokenizer.Tokenize(satExpression).ToList());

        if (tokens.AnyIsUnknown(out SatToken? unknownToken))
            return new SatParseError($"Unknown token '{unknownToken.Value}'", unknownToken.Index);

        var clauses = new List<ClauseDto>();
        while (tokens.CanPopToken())
        {
            if (tokens.PopToken() is { Type: not SatTokenType.LeftBracket } notLeftBracket)
                return new SatParseError("Expected '('", notLeftBracket.Index);

            var literals = new List<LiteralDto>();
            while (tokens.CanPopToken() && tokens.PopToken() is { Type: SatTokenType.Literal } literalToken)
            {
                string variableName = NameOf(literalToken);
                bool negated = IsNegated(literalToken);
                int variableNumber = GetVariableNumberOf(variableName);
                literals.Add(new LiteralDto(variableNumber, negated));
            }

            if (tokens.CannotPopToken() && tokens.PreviousToken() is { Type: not SatTokenType.RightBracket })
                return new SatParseError("Expected ')'", satExpression.Length);

            if (tokens.PreviousToken() is { Type: not SatTokenType.RightBracket } notRightBracket)
                return new SatParseError("Expected ')'", notRightBracket.Index);

            if (literals.Count == 0)
                return new SatParseError("No empty clauses are allowed.", tokens.PreviousToken().Index);

            clauses.Add(new ClauseDto(literals));
        }

        return clauses;
    }

    public void Clear()
    {
        _varId = 1;
        _symbolTable.Clear();
    }

    private int GetVariableNumberOf(string name)
    {
        if (!_symbolTable.TryGetValue(name, out int satVariable))
            _symbolTable[name] = satVariable = _varId++;

        return satVariable;
    }

    private static bool IsUnknown(SatToken token) => token.Type == SatTokenType.Unknown;

    private static bool IsNegated(SatToken token) => token.Value.StartsWith('~');

    private static string NameOf(SatToken token) => token.Value.Trim('~');

    private struct SatTokens(List<SatToken> tokens)
    {
        private int _currentIndex = 0;

        public bool CanPopToken () => _currentIndex < tokens.Count;

        public bool CannotPopToken() => !CanPopToken();

        public SatToken PopToken() => tokens[_currentIndex++];

        public SatToken PreviousToken() => tokens[_currentIndex - 1];

        public bool AnyIsUnknown([NotNullWhen(true)] out SatToken? firstUnknownToken)
        {
            firstUnknownToken = tokens.FirstOrDefault(IsUnknown);
            return firstUnknownToken is not null;
        }
    }
}