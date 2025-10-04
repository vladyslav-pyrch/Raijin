using System.Diagnostics.CodeAnalysis;
using FluentResults;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public static class BooleanExpressionParser
{
    private static readonly Dictionary<BooleanTokenType, int> Precedences = new()
    {
        { BooleanTokenType.Not, 5 },
        { BooleanTokenType.And, 4 },
        { BooleanTokenType.Nand, 4 },
        { BooleanTokenType.Or, 3 },
        { BooleanTokenType.Nor, 3 },
        { BooleanTokenType.Xor, 2 },
        { BooleanTokenType.Implication, 1 },
        { BooleanTokenType.ImplicationBackward, 1 },
        { BooleanTokenType.Equivalence, 0 }
    };

    private static readonly HashSet<BooleanTokenType> RightAssociativeOperators =
    [
        BooleanTokenType.Not,
        BooleanTokenType.Implication,
        BooleanTokenType.ImplicationBackward
    ];

    public static Result<IBooleanExpression> ParseExpression(string expression)
    {
        if (expression == string.Empty)
            return new BooleanExpressionParseError("The expression is empty.", 0);

        var tokens = new BooleanTokens(BooleanExpressionTokenizer.Tokenize(expression).ToList());

        if (tokens.AnyIsUnknown(out BooleanToken? token))
            return new BooleanExpressionParseError($"Unknown token '{token.Value}'", token.Index);

        Result<PostfixBooleanTokens> postfixResult = tokens.ShuntingYard();

        if (postfixResult.IsFailed)
            return postfixResult.ToResult<IBooleanExpression>();

        PostfixBooleanTokens postfixTokens = postfixResult.Value;
        return postfixTokens.ToExpression();
    }

    private static bool IsOperator(BooleanToken token) => Precedences.ContainsKey(token.Type);

    private static bool IsRightAssociative(BooleanTokenType type) => RightAssociativeOperators.Contains(type);

    private static bool IsLeftAssociative(BooleanTokenType type) => !IsRightAssociative(type);

    private static bool IsUnknown(BooleanToken token) => token.Type == BooleanTokenType.Unknown;

    private sealed class BooleanTokens(List<BooleanToken> tokens)
    {
        private int _position;

        public Result<PostfixBooleanTokens> ShuntingYard()
        {
            List<BooleanToken> postfix = [];
            Stack<BooleanToken> stack = [];

            while (HasMore)
            {
                BooleanToken? previous = PreviousToken();
                BooleanToken? next = NextToken();
                BooleanToken token = PopToken()!;

                if (token is { Type: BooleanTokenType.Not } &&
                    next is null or { Type: not BooleanTokenType.Variable and not BooleanTokenType.LeftBracket and not BooleanTokenType.Not })
                    return new BooleanExpressionParseError("'not' (~) operator must be followed by a variable, a left parenthesis, or another 'not' operator.", token.Index);

                if (previous != null && IsOperator(previous) && IsOperator(token) && token.Type != BooleanTokenType.Not)
                    return new BooleanExpressionParseError("Two operators in a row.", token.Index);

                switch (token.Type)
                {
                    case BooleanTokenType.Variable:
                    {
                        postfix.Add(token);
                        break;
                    }
                    case BooleanTokenType.LeftBracket:
                    {
                        stack.Push(token);
                        break;
                    }
                    case BooleanTokenType.RightBracket:
                    {
                        while (stack.Count > 0 && stack.Peek() is { Type: not BooleanTokenType.LeftBracket })
                            postfix.Add(stack.Pop());

                        if (stack.Count == 0 || stack.Peek() is { Type: not BooleanTokenType.LeftBracket })
                            return new BooleanExpressionParseError("Mismatched parentheses.", token.Index);
                        stack.Pop();
                        break;
                    }
                    case BooleanTokenType.And or BooleanTokenType.Nand or BooleanTokenType.Or or BooleanTokenType.Nor
                        or BooleanTokenType.Not or BooleanTokenType.Implication or BooleanTokenType.ImplicationBackward
                        or BooleanTokenType.Equivalence or BooleanTokenType.Xor:
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
                        return new BooleanExpressionParseError("Unknown operator.", token.Index);
                }
            }
            // what is somthing is laft on the stack
            while (stack.Count > 0)
            {
                if (stack.Peek() is { Type: BooleanTokenType.LeftBracket or BooleanTokenType.RightBracket })
                    return new BooleanExpressionParseError("Mismatched parentheses.", stack.Peek().Index);
                postfix.Add(stack.Pop());
            }

            return new PostfixBooleanTokens(postfix);
        }

        public bool AnyIsUnknown([NotNullWhen(true)] out BooleanToken? unknownToken)
        {
            unknownToken = tokens.FirstOrDefault(IsUnknown);
            return unknownToken is not null;
        }

        private BooleanToken? PopToken() => _position < tokens.Count ? tokens[_position++] : null;

        private BooleanToken? PreviousToken() => _position > 0 ? tokens[_position - 1] : null;

        private BooleanToken? NextToken() => _position < tokens.Count - 1 ? tokens[_position + 1] : null;

        private bool HasMore => _position < tokens.Count;
    }

    private sealed class PostfixBooleanTokens(List<BooleanToken> tokens)
    {
        public Result<IBooleanExpression> ToExpression()
        {
            var stack = new Stack<IBooleanExpression>();
            foreach (BooleanToken token in tokens)
            {
                switch (token.Type)
                {
                    case BooleanTokenType.Variable:
                        stack.Push(new Variable(token.Value));
                        break;
                    case BooleanTokenType.Not when stack.Count < 1:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'not' (~) operator.", token.Index);
                    case BooleanTokenType.Not:
                        stack.Push(new Negation(stack.Pop()));
                        break;
                    case BooleanTokenType.And when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'and' (& or *) operator.", token.Index);
                    case BooleanTokenType.And:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new Conjunction(left, right));
                        break;
                    }
                    case BooleanTokenType.Nand when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'nand' (~& or ~*) operator.", token.Index);
                    case BooleanTokenType.Nand:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new NegatedConjunction(left, right));
                        break;
                    }
                    case BooleanTokenType.Or when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'or' (| or +) operator.", token.Index);
                    case BooleanTokenType.Or:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new Disjunction(left, right));
                        break;
                    }
                    case BooleanTokenType.Nor when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'or' (~| or ~+) operator.", token.Index);
                    case BooleanTokenType.Nor:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new NegatedDisjunction(left, right));
                        break;
                    }
                    case BooleanTokenType.Xor when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'xor' (^) operator.", token.Index);
                    case BooleanTokenType.Xor:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new ExclusiveDisjunction(left, right));
                        break;
                    }
                    case BooleanTokenType.Implication when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'implication' (=>) operator.", token.Index);
                    case BooleanTokenType.Implication:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new Implication(left, right));
                        break;
                    }
                    case BooleanTokenType.ImplicationBackward when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'implication_backward' (<=) operator.", token.Index);
                    case BooleanTokenType.ImplicationBackward:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new Implication(right, left));
                        break;
                    }
                    case BooleanTokenType.Equivalence when stack.Count < 2:
                        return new BooleanExpressionParseError(
                            "Not enough operands for 'equivalence' (<=> or =) operator.", token.Index);
                    case BooleanTokenType.Equivalence:
                    {
                        IBooleanExpression right = stack.Pop();
                        IBooleanExpression left = stack.Pop();
                        stack.Push(new Equivalence(left, right));
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected token type {token.Type}.");
                }
            }

            if (stack.Count != 1)
                return new BooleanExpressionParseError($"Invalid expression: leftover operands after parsing.", -1);

            return Result.Ok(stack.Pop());
        }
    }
}