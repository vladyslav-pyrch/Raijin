using FluentResults;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;

public sealed class StringToBoolExprParser : IStringToBoolExprParser
{
    private static readonly Dictionary<BoolTokenType, int> Precedences = new()
    {
        {
            BoolTokenType.Not, 5 },
        {
            BoolTokenType.And, 4
        },
        {
            BoolTokenType.Or, 3
        },
        {
            BoolTokenType.Xor, 2
        },
        {
            BoolTokenType.Implication, 1
        },
        {
            BoolTokenType.ImplicationBackward, 1
        },
        {
            BoolTokenType.Equivalence, 0
        }
    };

    private static readonly HashSet<BoolTokenType> RightAssociativeOperators =
    [
        BoolTokenType.Not,
        BoolTokenType.Implication,
        BoolTokenType.ImplicationBackward
    ];

    public Result<BoolExpr> Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return Result.Fail(new Error("The expression is empty"));

        List<BoolToken> tokenList = StringToBoolExprLexer.Tokenize(expression).ToList();
        IEnumerable<BoolToken> unknownTokens = tokenList.Where(t => t.Type == BoolTokenType.Unknown);
        List<IError> unknownErrors = unknownTokens.Select(IError (t) => new Error($"Problem at {t.Index}. Unknown token '{t.Value}'"))
            .ToList();

        if (unknownErrors.Count > 0)
            return Result.Fail(unknownErrors);

        var tokens = new Tokens(tokenList);
        Result<PostfixTokens> postfixResult = tokens.ShuntingYard();
        if (postfixResult.IsFailed)
            return postfixResult.ToResult<BoolExpr>();

        return postfixResult.Value.ToExpression();
    }

    private static bool IsOperator(BoolToken boolToken) => Precedences.ContainsKey(boolToken.Type);

    private static bool IsRightAssociative(BoolTokenType type) => RightAssociativeOperators.Contains(type);

    private static bool IsLeftAssociative(BoolTokenType type) => !IsRightAssociative(type);

    private sealed class Tokens(List<BoolToken> tokens)
    {
        private int _position;

        private bool HasMore => _position < tokens.Count;

        public Result<PostfixTokens> ShuntingYard()
        {
            List<BoolToken> postfix = [];
            Stack<BoolToken> stack = [];

            while (HasMore)
            {
                BoolToken? previous = PreviousToken();
                BoolToken? next = NextToken();
                BoolToken boolToken = PopToken()!;

                if (boolToken is { Type: BoolTokenType.Not } &&
                    next is null or
                        { Type: not BoolTokenType.Variable and not BoolTokenType.LeftBracket and not BoolTokenType.Not })
                    return Result.Fail(new Error(
                        $"Problem at {boolToken.Index}. 'not' (~ or !) operator must be followed by a variable, a left parenthesis, or another 'not' operator"
                    ));

                if (previous != null && IsOperator(previous) && IsOperator(boolToken) && boolToken.Type != BoolTokenType.Not)
                    return Result.Fail(new Error($"Problem at {boolToken.Index}. Two operators in a row"));

                switch (boolToken.Type)
                {
                    case BoolTokenType.Variable or BoolTokenType.True or BoolTokenType.False:
                    {
                        postfix.Add(boolToken);
                        break;
                    }
                    case BoolTokenType.LeftBracket:
                    {
                        stack.Push(boolToken);
                        break;
                    }
                    case BoolTokenType.RightBracket:
                    {
                        while (stack.Count > 0 && stack.Peek() is { Type: not BoolTokenType.LeftBracket })
                            postfix.Add(stack.Pop());

                        if (stack.Count == 0 || stack.Peek() is { Type: not BoolTokenType.LeftBracket })
                            return Result.Fail(new Error($"Problem at {boolToken.Index}. Mismatched parentheses"));
                        stack.Pop();
                        break;
                    }
                    case BoolTokenType.And or BoolTokenType.Or or BoolTokenType.Not or BoolTokenType.Implication
                        or BoolTokenType.ImplicationBackward or BoolTokenType.Equivalence or BoolTokenType.Xor:
                    {
                        while (stack.Count > 0 && IsOperator(stack.Peek()) &&
                               (IsLeftAssociative(boolToken.Type) &&
                                Precedences[boolToken.Type] <= Precedences[stack.Peek().Type] ||
                                IsRightAssociative(boolToken.Type) &&
                                Precedences[boolToken.Type] < Precedences[stack.Peek().Type]))
                            postfix.Add(stack.Pop());
                        stack.Push(boolToken);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boolToken), $"Unexpected token type {boolToken.Type}");
                }
            }

            while (stack.Count > 0)
            {
                if (stack.Peek() is { Type: BoolTokenType.LeftBracket or BoolTokenType.RightBracket })
                    return Result.Fail(new Error($"Problem at {stack.Peek().Index}. Mismatched parentheses"));
                postfix.Add(stack.Pop());
            }

            return Result.Ok(new PostfixTokens(postfix));
        }

        private BoolToken? PopToken() => _position < tokens.Count ? tokens[_position++] : null;

        private BoolToken? PreviousToken() => _position > 0 ? tokens[_position - 1] : null;

        private BoolToken? NextToken() => _position < tokens.Count - 1 ? tokens[_position + 1] : null;
    }

    private sealed class PostfixTokens(List<BoolToken> tokens)
    {
        public Result<BoolExpr> ToExpression()
        {
            var stack = new Stack<BoolExpr>();
            foreach (BoolToken token in tokens)
                switch (token.Type)
                {
                    case BoolTokenType.True:
                        stack.Push(new ConstExpr(true));
                        break;
                    case BoolTokenType.False:
                        stack.Push(new ConstExpr(false));
                        break;
                    case BoolTokenType.Variable:
                        stack.Push(new BoolVar(token.Value));
                        break;
                    case BoolTokenType.Not when stack.Count < 1:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'not' (! or ~) operator"));
                    case BoolTokenType.Not:
                        stack.Push(new Not(stack.Pop()));
                        break;
                    case BoolTokenType.And when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'and' (& or *) operator"));
                    case BoolTokenType.And:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new And(left, right));
                        break;
                    }
                    case BoolTokenType.Or when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'or' (| or +) operator"));
                    case BoolTokenType.Or:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Or(left, right));
                        break;
                    }
                    case BoolTokenType.Xor when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'xor' (^) operator"));
                    case BoolTokenType.Xor:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Xor(left, right));
                        break;
                    }
                    case BoolTokenType.Implication when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'implication' (=> or ->) operator"));
                    case BoolTokenType.Implication:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Imply(left, right));
                        break;
                    }
                    case BoolTokenType.ImplicationBackward when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'implication_backward' (<= or <-) operator"));
                    case BoolTokenType.ImplicationBackward:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Imply(right, left));
                        break;
                    }
                    case BoolTokenType.Equivalence when stack.Count < 2:
                        return Result.Fail(new Error(
                            $"Problem at {token.Index}. Not enough operands for 'equivalence' (<->, <=>, or =) operator"));
                    case BoolTokenType.Equivalence:
                    {
                        BoolExpr right = stack.Pop();
                        BoolExpr left = stack.Pop();
                        stack.Push(new Equal(left, right));
                        break;
                    }
                    case BoolTokenType.LeftBracket or BoolTokenType.RightBracket or BoolTokenType.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected token type {token.Type}");
                }

            if (stack.Count != 1)
                return Result.Fail(new Error(
                    "The expression could not be parsed into a single boolean expression; leftover operands after parsing. Make sure to not use spaces between variables"));

            return Result.Ok(stack.Pop());
        }
    }
}