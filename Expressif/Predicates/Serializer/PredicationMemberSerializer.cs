using Expressif.Functions.Serializer;
using Expressif.Parsers;
using Expressif.Predicates.Combination;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Serializer
{
    public class PredicationMemberSerializer
    {
        private ParameterSerializer ParameterSerializer { get; }

        public PredicationMemberSerializer()
            : this(new ParameterSerializer()) { }

        public PredicationMemberSerializer(ParameterSerializer? parameterSerializer=null)
            => ParameterSerializer = parameterSerializer ?? new ParameterSerializer();

        public virtual string Serialize(PredicationMember member)
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

            var negation = member.Negation==typeof(NotOperator) ? "!" : string.Empty;

            var predicateName = member.Predicate.Name.ToKebabCase();

            var predicate = serializedParameters.Any()
                ? $"{predicateName}({string.Join(", ", serializedParameters)})"
                : predicateName;
            
            return $"{Serialize(member.Operator)}{negation}{predicate}";
        }

        public virtual string Serialize(Type? @operator)
        {
            if (@operator!= null && !@operator.GetInterfaces().Contains(typeof(ICombinationOperator)))
                throw new ArgumentException();
            
            return @operator == null
                ? string.Empty
                : $"|{@operator.Name.Replace("Operator", "").ToUpper()} ";
        }
    }
}
