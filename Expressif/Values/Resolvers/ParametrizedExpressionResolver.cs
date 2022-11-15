using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Resolvers
{
    public class ParametrizedExpressionResolver<T> : IScalarResolver<T>
    {
        private IFunction Expression { get; }
        private IScalarResolver Argument { get; }

        public ParametrizedExpressionResolver(IScalarResolver argument, IFunction expression)
            => (Argument, Expression) = (argument, expression);

        public T? Execute()
        {
            var result = Expression.Evaluate(Argument);
            result = result switch
            {
                decimal d when typeof(T).IsAssignableFrom(typeof(int)) => Convert.ToInt32(d),
                string s when typeof(T).IsAssignableFrom(typeof(DateTime)) => Convert.ToDateTime(s),
                _ => result,
            };
            return (T?)result;
        }
        object? IScalarResolver.Execute() => Execute();
    }
}
