using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed class TseitinTransformer
{
    public SatProblem Transform(BooleanFormula booleanFormula, out Dictionary<Variable, SatVariable> symbolTable)
    {
        IBooleanExpression desugaredExpression = booleanFormula.Expression.Desugar();
        var satProblem = new SatProblem();
        symbolTable = new Dictionary<Variable, SatVariable>();
        var varId = 1;

        RecursiveTransform(desugaredExpression, satProblem, symbolTable, newSatVariable: () => new SatVariable(varId++));

        return satProblem;
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
            default:
                throw new NotSupportedException($"Unsupported node: {expression.GetType().Name}");
        }
    }
}