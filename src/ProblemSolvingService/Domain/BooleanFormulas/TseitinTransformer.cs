using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed class TseitinTransformer
{
    public void Transform(BooleanFormula booleanFormula, SatProblem satProblem, out Dictionary<Variable, SatVariable> symbolTable)
    {
        IBooleanExpression desugaredExpression = booleanFormula.Expression.Desugar();
        symbolTable = new Dictionary<Variable, SatVariable>();
        var varId = 1;

        RecursiveTransform(desugaredExpression, satProblem, symbolTable, newSatVariable: () => new SatVariable(varId++));
    }

    private static SatVariable RecursiveTransform(IBooleanExpression expression, SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        Func<IBooleanExpression, SatVariable> transform = exp => RecursiveTransform(exp, satProblem, symbolTable, newSatVariable);

        switch (expression)
        {
            case Variable variable:
            {
                if (!symbolTable.TryGetValue(variable, out SatVariable? satVariable))
                    symbolTable[variable] = satVariable = newSatVariable();
                return satVariable;
            }
            case Negation negation:
            {
                SatVariable x = transform(negation.Expression);
                SatVariable notX = newSatVariable();

                satProblem.AddClause(Literal.Negated(x), Literal.Negated(notX));
                satProblem.AddClause(Literal.Affirmed(x), Literal.Affirmed(notX));

                return notX;
            }
            case Conjunction conjunction:
            {
                SatVariable x = transform(conjunction.Expression1);
                SatVariable y = transform(conjunction.Expression2);
                SatVariable xAndY = newSatVariable();

                satProblem.AddClause(Literal.Negated(xAndY), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Negated(xAndY), Literal.Affirmed(y));
                satProblem.AddClause(Literal.Affirmed(xAndY), Literal.Negated(x), Literal.Negated(y));

                return xAndY;
            }
            case NegatedConjunction negatedConjunction:
            {
                SatVariable x = transform(negatedConjunction.Expression1);
                SatVariable y = transform(negatedConjunction.Expression2);
                SatVariable xNandY = newSatVariable();

                satProblem.AddClause(Literal.Affirmed(xNandY), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Affirmed(xNandY), Literal.Affirmed(y));
                satProblem.AddClause(Literal.Negated(xNandY), Literal.Negated(x), Literal.Negated(y));

                return xNandY;

            }
            case Disjunction disjunction:
            {
                SatVariable x = transform(disjunction.Expression1);
                SatVariable y = transform(disjunction.Expression2);
                SatVariable xOrY = newSatVariable();

                satProblem.AddClause(Literal.Affirmed(xOrY), Literal.Negated(x));
                satProblem.AddClause(Literal.Affirmed(xOrY), Literal.Negated(y));
                satProblem.AddClause(Literal.Negated(xOrY), Literal.Affirmed(x), Literal.Affirmed(y));

                return xOrY;
            }
            case NegatedDisjunction negatedDisjunction:
            {
                SatVariable x = transform(negatedDisjunction.Expression1);
                SatVariable y = transform(negatedDisjunction.Expression2);
                SatVariable xNorY = newSatVariable();

                satProblem.AddClause(Literal.Negated(xNorY), Literal.Negated(x));
                satProblem.AddClause(Literal.Negated(xNorY), Literal.Negated(y));
                satProblem.AddClause(Literal.Affirmed(xNorY), Literal.Affirmed(x), Literal.Affirmed(y));

                return xNorY;
            }
            case Equivalence equivalence:
            {
                SatVariable x = transform(equivalence.Expression1);
                SatVariable y = transform(equivalence.Expression2);
                SatVariable xEqualY = newSatVariable();

                satProblem.AddClause(Literal.Affirmed(xEqualY), Literal.Negated(y), Literal.Negated(x));
                satProblem.AddClause(Literal.Affirmed(xEqualY), Literal.Affirmed(y), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Negated(xEqualY), Literal.Negated(y), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Negated(xEqualY), Literal.Affirmed(y), Literal.Negated(x));

                return xEqualY;
            }
            case ExclusiveDisjunction exclusiveDisjunction:
            {
                SatVariable x = transform(exclusiveDisjunction.Expression1);
                SatVariable y = transform(exclusiveDisjunction.Expression2);
                SatVariable xXorY = newSatVariable();

                satProblem.AddClause(Literal.Negated(xXorY), Literal.Negated(y), Literal.Negated(x));
                satProblem.AddClause(Literal.Negated(xXorY), Literal.Affirmed(y), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Affirmed(xXorY), Literal.Negated(y), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Affirmed(xXorY), Literal.Affirmed(y), Literal.Negated(x));

                return xXorY;

            }
            case Implication implication:
            {
                SatVariable x = transform(implication.Condition);
                SatVariable y = transform(implication.Consequence);
                SatVariable xImplyY = newSatVariable();

                satProblem.AddClause(Literal.Affirmed(xImplyY), Literal.Affirmed(x));
                satProblem.AddClause(Literal.Affirmed(xImplyY), Literal.Negated(y));
                satProblem.AddClause(Literal.Negated(xImplyY), Literal.Negated(x), Literal.Affirmed(y));

                return xImplyY;
            }
            default:
                throw new NotSupportedException($"Unsupported node: {expression.GetType().Name}");
        }
    }
}