using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Parsers;

namespace Expressif.Serializers;

public class ExpressionSerializer
{
    private FunctionSerializer FunctionSerializer { get; }
    private ParameterSerializer ParameterSerializer { get; }

    public ExpressionSerializer()
        : this(new FunctionSerializer()) { }
    public ExpressionSerializer(FunctionSerializer? functionSerializer = null)
        => (FunctionSerializer, ParameterSerializer) = (functionSerializer ?? new FunctionSerializer(), new ParameterSerializer());

    public virtual void Serialize(IExpression expression, ref StringBuilder stringBuilder)
    {
        switch (expression)
        {
            case Function f:
                FunctionSerializer.Serialize(f, ref stringBuilder);
                break;
            case OpenExpression exp:
                Serialize(exp, ref stringBuilder);
                break;
            case Parsers.ClosedExpression exp:
                Serialize(exp, ref stringBuilder);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public virtual void Serialize(OpenExpression expression, ref StringBuilder stringBuilder)
        => Serialize([.. expression.Members], ref stringBuilder);

    public virtual void Serialize(Parsers.ClosedExpression expression, ref StringBuilder stringBuilder)
    {
        stringBuilder.Append(ParameterSerializer.Serialize(expression.Parameter));
        SerializeContinuations(expression.Members, ref stringBuilder);
    }

    public virtual void Serialize(IExpression[] expressions, ref StringBuilder stringBuilder)
    {
        if (expressions.Length == 0)
            return;

        Serialize(expressions[0], ref stringBuilder);
        SerializeContinuations(expressions.Skip(1).OfType<Function>(), ref stringBuilder);
    }

    private void SerializeContinuations(IEnumerable<Function> functions, ref StringBuilder stringBuilder)
    {
        foreach (var function in functions)
        {
            if (function.Syntax == FunctionSyntax.MapShorthand)
            {
                stringBuilder.Append(" |> (");
                var expression = (OpenExpressionParameter)function.Parameters.Single();
                Serialize(expression.Expression, ref stringBuilder);
                stringBuilder.Append(')');
            }
            else
            {
                stringBuilder.Append(" | ");
                Serialize(function, ref stringBuilder);
            }
        }
    }

    public virtual string Serialize(IExpression expression)
        => Serialize([expression]);

    public virtual string Serialize(IExpression[] expressions)
    {
        var sb = new StringBuilder();
        Serialize(expressions, ref sb);
        return sb.ToString();
    }
}
