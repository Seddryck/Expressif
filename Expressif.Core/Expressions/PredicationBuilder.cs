using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Serialization;
using Expressif.Values.Special;
using System.Linq.Expressions;

namespace Expressif;

public class AbstractPredicationBuilder
{
    private IContext Context { get; }
    private IPredicationFactory Factory { get; }
    private PredicationSerializer Serializer { get; }

    protected AbstractPredicationBuilder(
        IPredicationFactory factory,
        IContext? context = null,
        PredicationSerializer? serializer = null)
        => (Factory, Context, Serializer) = (
            factory ?? throw new ArgumentNullException(nameof(factory)),
            context ?? new Context(),
            serializer ?? new PredicationSerializer());

    protected AbstractPredicationBuilder(AbstractPredicationBuilder builder)
        => (Context, Factory, Serializer, Pile) = (builder.Context, builder.Factory, builder.Serializer, builder.Pile);

    protected internal IPredication? Pile { get; set; }

    protected IPredication BuildNot(Type type, object?[] parameters)
        => new UnaryPredication(
            new UnaryOperator("!"),
            new SinglePredication(new Bindings.Function(type.Name, Parametrize(parameters))));

    public IPredicate Build()
    {
        if (Pile is null)
            throw new InvalidOperationException();
        return Factory.Instantiate(Pile, Context);
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

    public string Serialize()
    {
        if (Pile is null)
            throw new InvalidOperationException();

        return Serializer.Serialize(Pile);
    }
}

public class PredicationBuilder : AbstractPredicationBuilder
{
    public PredicationBuilder(
        IPredicationFactory factory,
        IContext? context = null,
        PredicationSerializer? serializer = null)
        : base(factory, context, serializer) { }

    public PredicationBuilderNext Create<T>()
        where T : IPredicate
        => Create(typeof(T), []);

    public PredicationBuilderNext Create<T>(params object?[] parameters)
        where T : IPredicate
        => Create(typeof(T), parameters);

    public PredicationBuilderNext Create<T>(params Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => Create(typeof(T), parameters);

    public PredicationBuilderNext Create(Type type, params object?[] parameters)
    {
        EnsurePredicate(type);
        Pile = new SinglePredication(new Bindings.Function(type.Name, Parametrize(parameters)));
        return new(this);
    }

    public PredicationBuilderNext Not<T>()
        where T : IPredicate
        => Not<T>([]);

    public PredicationBuilderNext Not<T>(params object?[] parameters)
        where T : IPredicate
        => Not(typeof(T), parameters);

    public PredicationBuilderNext Not<T>(params Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => Not(typeof(T), parameters);

    public PredicationBuilderNext Not(Type type, params object?[] parameters)
    {
        EnsurePredicate(type);
        Pile = BuildNot(type, Parametrize(parameters));
        return new(this);
    }

    private static void EnsurePredicate(Type type)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));
    }
}

public class PredicationBuilderNext : AbstractPredicationBuilder
{
    public PredicationBuilderNext(AbstractPredicationBuilder builder)
        : base(builder) { }

    public PredicationBuilderNext And<T>()
        where T : IPredicate
        => And(typeof(T), []);
    public PredicationBuilderNext And<T>(params object?[] parameters)
        where T : IPredicate
        => And(typeof(T), parameters);
    public PredicationBuilderNext And<T>(params Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => And(typeof(T), parameters);

    public PredicationBuilderNext And(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Bindings.Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    public PredicationBuilderNext And(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilderNext AndNot<T>()
        where T : IPredicate
        => AndNot(typeof(T), []);
    public PredicationBuilderNext AndNot<T>(params object?[] parameters)
        where T : IPredicate
        => AndNot(typeof(T), parameters);
    public PredicationBuilderNext AndNot<T>(Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => AndNot(typeof(T), parameters);

    public PredicationBuilderNext AndNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    public PredicationBuilderNext Or<T>()
        where T : IPredicate
        => Or(typeof(T), []);
    public PredicationBuilderNext Or<T>(params object?[] parameters)
        where T : IPredicate
        => Or(typeof(T), parameters);
    public PredicationBuilderNext Or<T>(Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => Or(typeof(T), parameters);

    public PredicationBuilderNext Or(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Bindings.Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return new(this);
    }

    public PredicationBuilderNext Or(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilderNext OrNot<T>()
        where T : IPredicate
        => OrNot(typeof(T), []);
    public PredicationBuilderNext OrNot<T>(params object?[] parameters)
        where T : IPredicate
        => OrNot(typeof(T), parameters);
    public PredicationBuilderNext OrNot<T>(Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => OrNot(typeof(T), parameters);

    public PredicationBuilderNext OrNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return new(this);
    }

    public PredicationBuilderNext Xor<T>()
        where T : IPredicate
        => Xor(typeof(T), []);
    public PredicationBuilderNext Xor<T>(params object?[] parameters)
        where T : IPredicate
        => Xor(typeof(T), parameters);
    public PredicationBuilderNext Xor<T>(Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => Xor(typeof(T), parameters);

    public PredicationBuilderNext Xor(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Bindings.Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return new(this);
    }

    public PredicationBuilderNext Xor(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilderNext XorNot<T>()
        where T : IPredicate
        => XorNot(typeof(T), []);
    public PredicationBuilderNext XorNot<T>(params object?[] parameters)
        where T : IPredicate
        => XorNot(typeof(T), parameters);
    public PredicationBuilderNext XorNot<T>(Expression<Func<IContext, object?>>[] parameters)
        where T : IPredicate
        => XorNot(typeof(T), parameters);

    public PredicationBuilderNext XorNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return new(this);
    }
}
