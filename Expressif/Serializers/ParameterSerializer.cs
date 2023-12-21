using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers
{
    public class ParameterSerializer
    {
        public virtual string Serialize(IParameter parameter)
        {
            return parameter switch
            {
                LiteralParameter l => l.Value.Any(
                    x => Grammar.AlongQuotedChars
                            .Union(Grammar.OpeningQuotedChars)
                            .Union(Grammar.ClosingQuotedChars).Contains(x))
                        ? $"\"{l.Value}\""
                        : l.Value,
                VariableParameter v => $"@{v.Name}",
                ObjectPropertyParameter op => $"[{op.Name}]",
                ObjectIndexParameter oi => $"#{oi.Index}",
                _ => throw new NotSupportedException()
            };
        }
    }
}
