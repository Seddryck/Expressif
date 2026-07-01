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
        };
    }

    public virtual void Serialize(OpenExpression expression, ref StringBuilder stringBuilder)
        => Serialize([.. expression.Members], ref stringBuilder);

    public virtual void Serialize(Parsers.ClosedExpression expression, ref StringBuilder stringBuilder)
    {
        stringBuilder.Append(ParameterSerializer.Serialize(expression.Parameter));
        if (!expression.Members.Any())
            return;

        stringBuilder.Append(" | ");
        Serialize([.. expression.Members], ref stringBuilder);
    }

    public virtual void Serialize(IExpression[] expressions, ref StringBuilder stringBuilder)
    {
        foreach (var expression in expressions)
        {
            Serialize(expression, ref stringBuilder);
            stringBuilder.Append(" | ");
        }
        stringBuilder.Remove(stringBuilder.Length - 3, 3);
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
