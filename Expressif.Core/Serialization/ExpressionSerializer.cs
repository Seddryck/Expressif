using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Bindings;

namespace Expressif.Serialization;

internal sealed class ExpressionSerializer
{
    private FunctionSerializer FunctionSerializer { get; }
    private ParameterSerializer ParameterSerializer { get; }

    public ExpressionSerializer()
        => (FunctionSerializer, ParameterSerializer) = (new FunctionSerializer(), new ParameterSerializer());

    public void Serialize(IBoundExpression expression, ref StringBuilder stringBuilder)
    {
        switch (expression)
        {
            case Function f:
                FunctionSerializer.Serialize(f, ref stringBuilder);
                break;
            case OpenExpression exp:
                Serialize(exp, ref stringBuilder);
                break;
            case Bindings.ClosedExpression exp:
                Serialize(exp, ref stringBuilder);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void Serialize(OpenExpression expression, ref StringBuilder stringBuilder)
        => Serialize([.. expression.Members], ref stringBuilder);

    public void Serialize(Bindings.ClosedExpression expression, ref StringBuilder stringBuilder)
    {
        stringBuilder.Append(ParameterSerializer.Serialize(expression.Parameter));
        SerializeContinuations(expression.Members, ref stringBuilder);
    }

    public void Serialize(IBoundExpression[] expressions, ref StringBuilder stringBuilder)
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
            else if (function.Syntax == FunctionSyntax.GroupMapShorthand)
            {
                stringBuilder.Append(" |#> ");
                var expression = (OpenExpressionParameter)function.Parameters.Single();
                Serialize(expression.Expression, ref stringBuilder);
            }
            else
            {
                stringBuilder.Append(" | ");
                Serialize(function, ref stringBuilder);
            }
        }
    }

    public string Serialize(IBoundExpression expression)
        => Serialize([expression]);

    public string Serialize(IBoundExpression[] expressions)
    {
        var sb = new StringBuilder();
        Serialize(expressions, ref sb);
        return sb.ToString();
    }
}
