using Expressif.Functions;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Resolvers;

internal class InputExpressionResolver<T> : IScalarResolver<T>
{
    private IFunction Expression { get; }
    private Func<object> Argument { get; }

    public InputExpressionResolver(Func<object> argument, IFunction expression)
        => (Argument, Expression) = (argument, expression);

    public T? Execute() => new Caster().Cast<T>(Expression.Evaluate(Argument.Invoke()));
        
    object? IScalarResolver.Execute() => Execute();
}
