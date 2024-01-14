using Expressif.Functions;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers;

public class ParameterSerializer
{
    public virtual string Serialize(IParameter parameter)
    {
        var stringBuilder = new StringBuilder();
        Serialize(parameter, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public virtual void Serialize(IParameter parameter, ref StringBuilder stringBuilder)
    { 
        switch (parameter)
        {
            case LiteralParameter l:
                stringBuilder.Append(
                    l.Value.Any(
                        x => Grammar.AlongQuotedChars
                                .Union(Grammar.OpeningQuotedChars)
                                .Union(Grammar.ClosingQuotedChars).Contains(x))
                    ? $"`{l.Value}`"
                    : l.Value); break;
            case VariableParameter v: stringBuilder.Append($"@{v.Name}"); break;
            case ObjectPropertyParameter op: stringBuilder.Append($"[{op.Name}]"); break;
            case ObjectIndexParameter oi: stringBuilder.Append($"#{oi.Index}"); break;
            default: throw new NotSupportedException();
        }
    }
}
