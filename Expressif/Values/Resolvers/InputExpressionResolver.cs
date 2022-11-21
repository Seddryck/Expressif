using Expressif.Functions;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Resolvers
{
    public class InputExpressionResolver<T> : IScalarResolver<T>
    {
        private IFunction Expression { get; }
        private IScalarResolver Argument { get; }

        public InputExpressionResolver(IScalarResolver argument, IFunction expression)
            => (Argument, Expression) = (argument, expression);

        public T? Execute() => new Caster().Cast<T>(Expression.Evaluate(Argument));
            
        object? IScalarResolver.Execute() => Execute();
    }
}
