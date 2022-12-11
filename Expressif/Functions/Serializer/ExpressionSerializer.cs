using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Serializer
{
    public class ExpressionSerializer
    {
        private ExpressionMemberSerializer ExpressionMemberSerializer { get; }

        public ExpressionSerializer()
            : this(new ExpressionMemberSerializer()) {}
        public ExpressionSerializer(ExpressionMemberSerializer? expressionMemberSerializer = null)
            => ExpressionMemberSerializer = expressionMemberSerializer ?? new ExpressionMemberSerializer();

        public virtual string Serialize(ExpressionBuilder builder)
        {
            var expression = new StringBuilder();
            foreach (var member in builder.Pile)
            {

                expression.Append(member switch
                {
                    ExpressionMember em => ExpressionMemberSerializer.Serialize(em),
                    ExpressionBuilder eb => $"{{ {Serialize(eb)} }}",
                    _ => throw new NotSupportedException()
                }).Append(" | ");
            }
            expression.Remove(expression.Length - 3, 3);
            return expression.ToString();
        }
    }
}
