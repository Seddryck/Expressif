
using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Operators;
using Expressif.Serializers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif;

public class ExpressionBuilder
{
    private IContext Context { get; }
    private ExpressionFactory Factory { get; }
    private ExpressionSerializer Serializer { get; }

    public ExpressionBuilder()
        : this(new Context()) { }
    public ExpressionBuilder(IContext? context = null, ExpressionFactory? factory = null, ExpressionSerializer? serializer = null)
        => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new ExpressionFactory(), serializer ?? new ExpressionSerializer());

    private Queue<IExpressionParsable> Pile { get; } = new();

    public ExpressionBuilder Chain<T>() where T : IFunction
        => Chain(typeof(T), []);

    public ExpressionBuilder Chain<T>(params object?[] parameters) where T : IFunction
        => Chain(typeof(T), parameters);

    public ExpressionBuilder Chain<T>(params Expression<Func<IContext, object?>>[] parameters) where T : IFunction
        => Chain(typeof(T), parameters);

    public ExpressionBuilder Chain(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IFunction)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IFunction)}'. Only types implementing this interface can be chained to create an expression.", nameof(type));

        Pile.Enqueue(new FunctionMeta(type.Name, Parametrize(parameters)));
        return this;
    }

    public ExpressionBuilder Chain(ExpressionBuilder builder)
    {
        foreach (var element in builder.Pile)
            Pile.Enqueue(element);
        return this;
    }

    protected virtual IParameter[] Parametrize(object?[] parameters)
    {
        var typedParameters = new List<IParameter>();
        foreach (var parameter in parameters)
        {
            typedParameters.Add(parameter switch
            {
                IParameter p => p,
                Expression<Func<IContext, object?>> expression => new ContextParameter(expression.Compile()),
                _ => new LiteralParameter(parameter?.ToString() ?? new Null().Keyword)
            });
        }
        return [.. typedParameters];
    }

    public IFunction Build()
    {
        IFunction? function = null;
        if (Pile.Count == 0)
            throw new InvalidOperationException();

        while (Pile.Count != 0)
        {
            var member = Pile.Dequeue() switch
            {
                FunctionMeta f => Factory.Instantiate(f.Name, f.Parameters, Context),
                ExpressionBuilder b => b.Build(),
                IFunction f => f,
                _ => throw new NotSupportedException()
            };
            function = function is null ? member : new ChainOperator(new[] { function, member });
        }

        return function!;
    }

    public string Serialize()
    {
        if (Pile.Count == 0)
            throw new InvalidOperationException();

        return Serializer.Serialize([.. Pile]);
    }
}
