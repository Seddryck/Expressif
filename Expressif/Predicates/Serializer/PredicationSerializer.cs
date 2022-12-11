using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Serializer
{
    public class PredicationSerializer
    {
        private PredicationMemberSerializer MemberSerializer { get; }

        public PredicationSerializer()
            : this(new()) { }
        public PredicationSerializer(PredicationMemberSerializer? predicationMemberSerializer = null)
            => MemberSerializer = predicationMemberSerializer ?? new();

        public virtual string Serialize(PredicationBuilder builder)
        {
            var predication = new StringBuilder();
            foreach (var member in builder.Pile)
            {

                predication.Append(member switch
                {
                    PredicationMember em => MemberSerializer.Serialize(em),
                    SubPredicationMember sub => $"{MemberSerializer.Serialize(sub.Operator)}{{ {Serialize(sub.PredicationBuilder)} }}",
                    _ => throw new NotSupportedException()
                }).Append(' ');
            }
            predication.Remove(predication.Length - 1, 1);
            return predication.ToString();
        }
    }
}
