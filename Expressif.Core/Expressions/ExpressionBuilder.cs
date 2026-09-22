using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Serialization;
using Expressif.Values.Special;
using System.Linq.Expressions;

namespace Expressif;

public class ExpressionBuilder
{
    private IContext Context { get; }
    private FunctionFactory Factory { get; }
    private ExpressionSerializer Serializer { get; }

    public ExpressionBuilder(FunctionFactory factory)
        : this(factory, new Context()) { }

    public ExpressionBuilder(
        FunctionFactory factory,
        IContext? context)
        => (Factory, Context, Serializer) = (
            factory ?? throw new ArgumentNullException(nameof(factory)),
            context ?? new Context(),
            new ExpressionSerializer());

    private Queue<IBoundExpression> Pile { get; } = new();

    public ExpressionBuilder Chain<T>()
        where T : IFunction
        => Chain(typeof(T), []);

    public ExpressionBuilder Chain<T>(params object?[] parameters)
        where T : IFunction
        => Chain(typeof(T), parameters);

    public ExpressionBuilder Chain<T>(params Expression<Func<IContext, object?>>[] parameters)
        where T : IFunction
        => Chain(typeof(T), parameters);

    public ExpressionBuilder Chain(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IFunction)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IFunction)}'. Only types implementing this interface can be chained to create an expression.", nameof(type));

        Pile.Enqueue(new Bindings.Function(type.Name, Parametrize(parameters)));
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
                _ => new LiteralParameter(parameter?.ToString() ?? Null.Instance.Keyword)
            });
        }
        return [.. typedParameters];
    }

    public ExpressionBuilder Chain(ExpressionBuilder builder)
    {
        foreach (var element in builder.Pile)
            Pile.Enqueue(element);
        return this;
    }

    public ExpressionBuilder Chain(Bindings.Function function)
    {
        Pile.Enqueue(function);
        return this;
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
                Bindings.Function f => Factory.Instantiate(f.Name, f.Parameters, Context),
                ExpressionBuilder b => b.Build(),
                IFunction f => f,
                _ => throw new NotSupportedException()
            };
            function = function is null ? member : new ChainFunction([function, member]);
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
