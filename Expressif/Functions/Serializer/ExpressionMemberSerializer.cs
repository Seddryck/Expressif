using Expressif.Parsers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Serializer
{
    public class ExpressionMemberSerializer
    {
        private ParameterSerializer ParameterSerializer { get; }

        public ExpressionMemberSerializer()
            : this(new ParameterSerializer()) { }

        public ExpressionMemberSerializer(ParameterSerializer? parameterSerializer = null)
            => ParameterSerializer = parameterSerializer ?? new ParameterSerializer();

        public virtual string Serialize(ExpressionMember member)
        {
            var serializedParameters = new List<string>();
            foreach (var parameter in member.Parameters)
            {
                serializedParameters.Add(ParameterSerializer.Serialize(parameter switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(parameter?.ToString() ?? new Null().Keyword)
                }));
            }
            var function = member.Type.Name.ToKebabCase();
            if (serializedParameters.Any())
                return $"{function}({string.Join(", ", serializedParameters)})";
            else
                return function;
        }
    }
}
