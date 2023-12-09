using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Functions.Serializer;
using Expressif.Parsers;
using Expressif.Predicates.Operators;

namespace Expressif.Predicates.Serializer
{
    public class SinglePredicationSerializer
    {
        private ParameterSerializer ParameterSerializer { get; set; }

        public SinglePredicationSerializer()
            : this(new()) { }
        public SinglePredicationSerializer(ParameterSerializer? parameterSerializer = null)
            => ParameterSerializer = parameterSerializer ?? new();

        internal virtual string Serialize(SinglePredication predication)
        {
            var stringBuilder = new StringBuilder();
            Serialize(predication, ref stringBuilder);
            return stringBuilder.ToString();
        }

        public virtual void Serialize(SinglePredication predication, ref StringBuilder stringBuilder)
            => Serialize(predication.Members[0], ref stringBuilder);

        protected virtual void Serialize(Function predicate, ref StringBuilder stringBuilder)
        {
            stringBuilder.Append(predicate.Name.ToKebabCase());
            if (predicate.Parameters.Any())
            {
                stringBuilder.Append('(');
                foreach (var parameter in predicate.Parameters)
                {
                    stringBuilder.Append(ParameterSerializer.Serialize(parameter));
                    stringBuilder.Append(", ");
                }
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(')');
            }
        }
    }
}
