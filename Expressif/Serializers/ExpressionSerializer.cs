using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Parsers;

namespace Expressif.Serializers;

public class ExpressionSerializer
{
    private FunctionSerializer FunctionSerializer { get; }

    public ExpressionSerializer()
        : this(new FunctionSerializer()) { }
    public ExpressionSerializer(FunctionSerializer? functionSerializer = null)
        => FunctionSerializer = functionSerializer ?? new FunctionSerializer();

    public virtual void Serialize(IExpressionParsable expression, ref StringBuilder stringBuilder)
    {
        switch (expression)
        {
            case FunctionMeta f:
                FunctionSerializer.Serialize(f, ref stringBuilder);
                break;
            case ExpressionMeta exp:
                Serialize(exp.Members, ref stringBuilder);
                break;
            default:
                throw new NotSupportedException();
        };
    }

    public virtual void Serialize(IExpressionParsable[] expressions, ref StringBuilder stringBuilder)
    {
        foreach (var expression in expressions)
        {
            Serialize(expression, ref stringBuilder);
            stringBuilder.Append(" | ");
        }
        stringBuilder.Remove(stringBuilder.Length - 3, 3);
    }

    public virtual string Serialize(IExpressionParsable expression)
        => Serialize([expression]);

    public virtual string Serialize(IExpressionParsable[] expressions)
    {
        var sb = new StringBuilder();
        Serialize(expressions, ref sb);
        return sb.ToString();
    }
}
