using System.Diagnostics.CodeAnalysis;
using FluentResults;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public static class ConstrainParser
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

    public static Result<ExpressionNode> ParseExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new Error("The expression is empty");

        var tokens = new Tokens(ConstrainTokenizer.Tokenize(expression).ToList());

        if (tokens.AnyIsUnknown(out Token? token))
            return new Error($"Problem at {token.Index}. Unknown token '{token.Value}'");

        Result<PostfixTokens> postfixResult = tokens.ShuntingYard();

        if (postfixResult.IsFailed)
            return postfixResult.ToResult<ExpressionNode>();

        PostfixTokens postfixTokens = postfixResult.Value;
        return postfixTokens.ToExpression();
    }

    private static bool IsOperator(Token token) => Precedences.ContainsKey(token.Type);

    private static bool IsRightAssociative(TokenType type) => RightAssociativeOperators.Contains(type);

    private static bool IsLeftAssociative(TokenType type) => !IsRightAssociative(type);

    private static bool IsUnknown(Token token) => token.Type == TokenType.Unknown;

    private sealed class Tokens(List<Token> tokens)
    {
        private int _position;

        public Result<PostfixTokens> ShuntingYard()
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
                    return new Error(
                        $"Problem at {token.Index}. 'not' (~) operator must be followed by a variable, a left parenthesis, or another 'not' operator"
                    );

                if (previous != null && IsOperator(previous) && IsOperator(token) && token.Type != TokenType.Not)
                    return new Error($"Problem at {token.Index}. Two operators in a row");

                switch (token.Type)
                {
                    case TokenType.Variable:
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
                            return new Error($"Problem at {token.Index}. Mismatched parentheses");
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
                    return new Error($"Problem at {stack.Peek().Index}. Mismatched parentheses");
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

        private bool HasMore => _position < tokens.Count;
    }

    private sealed class PostfixTokens(List<Token> tokens)
    {
        public Result<ExpressionNode> ToExpression()
        {
            var stack = new Stack<ExpressionNode>();
            foreach (Token token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Variable:
                    {
                        string[] parts = token.Value.Split("_is_");
                        if (parts.Length != 2)
                            return new Error(
                                $"Problem at {token.Index}. Variable token '{token.Value}' is not in the correct format 'VariableName_is_StateName'");
                        string variableName = parts[0];
                        string stateName = parts[1];
                        stack.Push(new StateNode(variableName, stateName));
                        break;
                    }
                    case TokenType.Not when stack.Count < 1:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'not' (~) operator");
                    case TokenType.Not:
                        stack.Push(new Negation(stack.Pop()));
                        break;
                    case TokenType.And when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'and' (& or *) operator");
                    case TokenType.And:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new Conjunction(left, right));
                        break;
                    }
                    case TokenType.Or when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'or' (| or +) operator");
                    case TokenType.Or:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new Disjunction(left, right));
                        break;
                    }
                    case TokenType.Xor when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'xor' (^) operator");
                    case TokenType.Xor:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new ExclusiveDisjunction(left, right));
                        break;
                    }
                    case TokenType.Implication when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'implication' (->) operator");
                    case TokenType.Implication:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new Implication(left, right));
                        break;
                    }
                    case TokenType.ImplicationBackward when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'implication_backward' (<-) operator");
                    case TokenType.ImplicationBackward:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new Implication(right, left));
                        break;
                    }
                    case TokenType.Equivalence when stack.Count < 2:
                        return new Error(
                            $"Problem at {token.Index}. Not enough operands for 'equivalence' (<-> or =) operator");
                    case TokenType.Equivalence:
                    {
                        ExpressionNode right = stack.Pop();
                        ExpressionNode left = stack.Pop();
                        stack.Push(new Equivalence(left, right));
                        break;
                    }
                    case TokenType.LeftBracket or TokenType.RightBracket or TokenType.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected token type {token.Type}");
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException(
                    "The expression could not be parsed into a single boolean expression; leftover operands after parsing");

            return Result.Ok(stack.Pop());
        }
    }
}