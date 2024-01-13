using Expressif.Predicates;
using Expressif.Parsers;
using Expressif.Values.Special;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Expressif;

public class AbstractPredicationBuilder
{
    private IContext Context { get; }
    private PredicationFactory Factory { get; }
    private PredicationSerializer Serializer { get; }

    protected AbstractPredicationBuilder(IContext? context, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new(), serializer ?? new());

    protected AbstractPredicationBuilder(AbstractPredicationBuilder builder)
        => (Context, Factory, Serializer, Pile) = (builder.Context, builder.Factory, builder.Serializer, builder.Pile);

    protected internal IPredication? Pile { get; set; }

    protected IPredication BuildNot(Type type, object?[] parameters)
    => new UnaryPredication(new UnaryOperator("!")
            , new SinglePredication(new Function(type.Name, Parametrize(parameters)))
        );

    public IPredicate Build()
    {
        if (Pile is null)
            throw new InvalidOperationException();
        var predicate = Factory.Instantiate(Pile, Context);
        return predicate;
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
    public PredicationBuilder()
        : this(new Context()) { }
    public PredicationBuilder(IContext? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        : base(context, factory, serializer) { }

    public PredicationBuilderNext Create<P>()
        where P : IPredicate
      => Create(typeof(P), []);

    public PredicationBuilderNext Create<P>(params object?[] parameters)
        where P : IPredicate
      => Create(typeof(P), parameters);

    public PredicationBuilderNext Create<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
       => Create(typeof(P), parameters);

    public PredicationBuilderNext Create(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));

        Pile = new SinglePredication(new Function(type.Name, Parametrize(parameters)));
        return new(this);
    }

    public PredicationBuilderNext Not<P>()
        where P : IPredicate
        => Not<P>([]);

    public PredicationBuilderNext Not<P>(params object?[] parameters)
        where P : IPredicate
        => Not(typeof(P), parameters);

    public PredicationBuilderNext Not<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => Not(typeof(P), parameters);

    public PredicationBuilderNext Not(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));
        
        Pile = BuildNot(type, Parametrize(parameters));
        return new(this);
    }
}

public class PredicationBuilderNext : AbstractPredicationBuilder
{
    public PredicationBuilderNext(AbstractPredicationBuilder builder)
        : base(builder) { }

    #region And

    public PredicationBuilderNext And<P>()
        where P : IPredicate
        => And(typeof(P), []);

    public PredicationBuilderNext And<P>(params object?[] parameters)
        where P : IPredicate
        => And(typeof(P), parameters);

    public PredicationBuilderNext And<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => And(typeof(P), parameters);

    public PredicationBuilderNext And(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    public PredicationBuilderNext And(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, builder.Pile!);
        return this;
    }

    #endregion

    #region AndNot

    public PredicationBuilderNext AndNot<P>()
        where P : IPredicate
        => AndNot(typeof(P), []);

    public PredicationBuilderNext AndNot<P>(params object?[] parameters)
        where P : IPredicate
        => AndNot(typeof(P), parameters);

    public PredicationBuilderNext AndNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => AndNot(typeof(P), parameters);

    public PredicationBuilderNext AndNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    #endregion

    #region Or

    public PredicationBuilderNext Or<P>()
        where P : IPredicate
        => Or(typeof(P), []);

    public PredicationBuilderNext Or<P>(params object?[] parameters)
        where P : IPredicate
        => Or(typeof(P), parameters);

    public PredicationBuilderNext Or<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => Or(typeof(P), parameters);

    public PredicationBuilderNext Or(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return new(this);
    }

    public PredicationBuilderNext Or(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, builder.Pile!);
        return this;
    }

    #endregion

    #region OrNot

    public PredicationBuilderNext OrNot<P>()
        where P : IPredicate
        => OrNot(typeof(P), []);

    public PredicationBuilderNext OrNot<P>(params object?[] parameters)
        where P : IPredicate
        => OrNot(typeof(P), parameters);

    public PredicationBuilderNext OrNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => OrNot(typeof(P), parameters);

    public PredicationBuilderNext OrNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return new(this);
    }

    #endregion

    #region Xor

    public PredicationBuilderNext Xor<P>()
        where P : IPredicate
        => Xor(typeof(P), []);

    public PredicationBuilderNext Xor<P>(params object?[] parameters)
        where P : IPredicate
        => Xor(typeof(P), parameters);

    public PredicationBuilderNext Xor<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => Xor(typeof(P), parameters);

    public PredicationBuilderNext Xor(Type type, params object?[] parameters)
    {
        var right = new SinglePredication(new Function(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return new(this);
    }

    public PredicationBuilderNext Xor(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, builder.Pile!);
        return this;
    }

    #endregion

    #region XorNot

    public PredicationBuilderNext XorNot<P>()
        where P : IPredicate
        => XorNot(typeof(P), []);

    public PredicationBuilderNext XorNot<P>(params object?[] parameters)
        where P : IPredicate
        => XorNot(typeof(P), parameters);

    public PredicationBuilderNext XorNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => XorNot(typeof(P), parameters);

    public PredicationBuilderNext XorNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return new(this);
    }

    #endregion
}
