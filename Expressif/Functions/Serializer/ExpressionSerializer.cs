using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Parsers;

namespace Expressif.Functions.Serializer
{
    public class ExpressionSerializer
    {
        private FunctionSerializer FunctionSerializer { get; }

        public ExpressionSerializer()
            : this(new FunctionSerializer()) { }
        public ExpressionSerializer(FunctionSerializer? functionSerializer = null)
            => FunctionSerializer = functionSerializer ?? new FunctionSerializer();

        public virtual void Serialize(IExpression expression, ref StringBuilder stringBuilder)
        {
            switch (expression)
            {
                case Parsers.Function f:
                    FunctionSerializer.Serialize(f, ref stringBuilder);
                    break;
                case Parsers.Expression exp:
                    Serialize(exp, ref stringBuilder);
                    break;
                default:
                    throw new NotSupportedException();
            };
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
}
