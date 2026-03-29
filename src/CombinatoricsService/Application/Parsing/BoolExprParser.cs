using System.Diagnostics.CodeAnalysis;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Application.Parsing;

/// <summary>
/// Parses a Boolean expression string into an immutable <see cref="BoolExpr"/> AST
/// using the Shunting-Yard algorithm.
/// </summary>
public sealed class BoolExprParser : IBoolExprParser
{
    private static readonly Dictionary<TokenType, int> Precedences = new()
    {
        { TokenType.Not, 5 },
        { TokenType.And, 4 },
        { TokenType.Or, 3 },
        { TokenType.Xor, 2 },
        { TokenType.Implication, 1 },
        { TokenType.ImplicationBackward, 1 },
        { TokenType.Equivalence, 0 }
    };

    private static readonly HashSet<TokenType> RightAssociativeOperators =
    [
        TokenType.Not,
        TokenType.Implication,
        TokenType.ImplicationBackward
    ];

    /// <inheritdoc />
    public BoolExpr Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ParsingException("The expression is empty");

        var tokens = new Tokens(BoolExprLexer.Tokenize(expression).ToList());

        if (tokens.AnyIsUnknown(out Token? token))
            throw new ParsingException($"Problem at {token.Index}. Unknown token '{token.Value}'");

        PostfixTokens postfix = tokens.ShuntingYard();
        return postfix.ToExpression();
    }

    private static bool IsOperator(Token token) => Precedences.ContainsKey(token.Type);

    private static bool IsRightAssociative(TokenType type) => RightAssociativeOperators.Contains(type);

    private static bool IsLeftAssociative(TokenType type) => !IsRightAssociative(type);

    private static bool IsUnknown(Token token) => token.Type == TokenType.Unknown;

    private sealed class Tokens(List<Token> tokens)
    {
        private int _position;

        private bool HasMore => _position < tokens.Count;

        public PostfixTokens ShuntingYard()
        {
            List<Token> postfix = [];
            Stack<Token> stack = [];

            while (HasMore)
            {
                Token? previous = PreviousToken();
                Token? next = NextToken();
                Token token = PopToken()!;

                if (token is { Type: TokenType.Not } &&
                    next is null or
                        { Type: not TokenType.Variable and not TokenType.LeftBracket and not TokenType.Not })
                    throw new ParsingException(
                        $"Problem at {token.Index}. 'not' (~) operator must be followed by a variable, a left parenthesis, or another 'not' operator"
                    );

                if (previous != null && IsOperator(previous) && IsOperator(token) && token.Type != TokenType.Not)
                    throw new ParsingException($"Problem at {token.Index}. Two operators in a row");

                switch (token.Type)
                {
                    case TokenType.Variable or TokenType.True or TokenType.False:
                    {
                        postfix.Add(token);
                        break;
                    }
                    case TokenType.LeftBracket:
                    {
                        stack.Push(token);
                        break;
                    }
                    case TokenType.RightBracket:
                    {
                        while (stack.Count > 0 && stack.Peek() is { Type: not TokenType.LeftBracket })
                            postfix.Add(stack.Pop());

                        if (stack.Count == 0 || stack.Peek() is { Type: not TokenType.LeftBracket })
                            throw new ParsingException($"Problem at {token.Index}. Mismatched parentheses");
                        stack.Pop();
                        break;
                    }
                    case TokenType.And or TokenType.Or or TokenType.Not or TokenType.Implication
                        or TokenType.ImplicationBackward or TokenType.Equivalence or TokenType.Xor:
                    {
                        while (stack.Count > 0 && IsOperator(stack.Peek()) &&
                               ((IsLeftAssociative(token.Type) &&
                                 Precedences[token.Type] <= Precedences[stack.Peek().Type]) ||
                                (IsRightAssociative(token.Type) &&
                                 Precedences[token.Type] < Precedences[stack.Peek().Type])))
                            postfix.Add(stack.Pop());
                        stack.Push(token);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected token type {token.Type}");
                }
            }

            while (stack.Count > 0)
            {
                if (stack.Peek() is { Type: TokenType.LeftBracket or TokenType.RightBracket })
                    throw new ParsingException($"Problem at {stack.Peek().Index}. Mismatched parentheses");
                postfix.Add(stack.Pop());
            }

            return new PostfixTokens(postfix);
        }

        public bool AnyIsUnknown([NotNullWhen(true)] out Token? unknownToken)
        {
            unknownToken = tokens.FirstOrDefault(IsUnknown);
            return unknownToken is not null;
        }

        private Token? PopToken() => _position < tokens.Count ? tokens[_position++] : null;

        private Token? PreviousToken() => _position > 0 ? tokens[_position - 1] : null;

        private Token? NextToken() => _position < tokens.Count - 1 ? tokens[_position + 1] : null;
    }

    private sealed class PostfixTokens(List<Token> tokens)
    {
        public BoolExpr ToExpression()
        {
            var stack = new Stack<BoolExpr>();
            foreach (Token token in tokens)
                switch (token.Type)
                {
                    case TokenType.True:
                        stack.Push(new ConstExpr(true));
                        break;
                    case TokenType.False:
                        stack.Push(new ConstExpr(false));
                        break;
                    case TokenType.Variable:
                        stack.Push(new BoolVar(token.Value));
                        break;
                    case TokenType.Not when stack.Count < 1:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'not' (~) operator");
                    case TokenType.Not:
                        stack.Push(new Not(stack.Pop()));
                        break;
                    case TokenType.And when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'and' (& or *) operator");
                    case TokenType.And:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new And(left, right));
                        break;
                    }
                    case TokenType.Or when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'or' (| or +) operator");
                    case TokenType.Or:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Or(left, right));
                        break;
                    }
                    case TokenType.Xor when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'xor' (^) operator");
                    case TokenType.Xor:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Xor(left, right));
                        break;
                    }
                    case TokenType.Implication when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'implication' (->) operator");
                    case TokenType.Implication:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Imply(left, right));
                        break;
                    }
                    case TokenType.ImplicationBackward when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'implication_backward' (<-) operator");
                    case TokenType.ImplicationBackward:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Imply(right, left));
                        break;
                    }
                    case TokenType.Equivalence when stack.Count < 2:
                        throw new ParsingException(
                            $"Problem at {token.Index}. Not enough operands for 'equivalence' (<-> or =) operator");
                    case TokenType.Equivalence:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Equal(left, right));
                        break;
                    }
                    case TokenType.LeftBracket or TokenType.RightBracket or TokenType.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected token type {token.Type}");
                }

            if (stack.Count != 1)
                throw new InvalidOperationException(
                    "The expression could not be parsed into a single boolean expression; leftover operands after parsing");

            return stack.Pop();
        }
    }
}
