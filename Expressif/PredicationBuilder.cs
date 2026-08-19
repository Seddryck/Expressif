using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Parsers;
using Expressif.Predicates;
using Expressif.Serializers;
using Expressif.Values.Special;

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

    public PredicationBuilderNext Create<TPredicate>()
        where TPredicate : IPredicate
      => Create(typeof(TPredicate), []);

    public PredicationBuilderNext Create<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
      => Create(typeof(TPredicate), parameters);

    public PredicationBuilderNext Create<TPredicate>(params Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
       => Create(typeof(TPredicate), parameters);

    public PredicationBuilderNext Create(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));

        Pile = new SinglePredication(new Function(type.Name, Parametrize(parameters)));
        return new(this);
    }

    public PredicationBuilderNext Not<TPredicate>()
        where TPredicate : IPredicate
        => Not<TPredicate>([]);

    public PredicationBuilderNext Not<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => Not(typeof(TPredicate), parameters);

    public PredicationBuilderNext Not<TPredicate>(params Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => Not(typeof(TPredicate), parameters);

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

    public PredicationBuilderNext And<TPredicate>()
        where TPredicate : IPredicate
        => And(typeof(TPredicate), []);

    public PredicationBuilderNext And<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => And(typeof(TPredicate), parameters);

    public PredicationBuilderNext And<TPredicate>(params Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => And(typeof(TPredicate), parameters);

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

    public PredicationBuilderNext AndNot<TPredicate>()
        where TPredicate : IPredicate
        => AndNot(typeof(TPredicate), []);

    public PredicationBuilderNext AndNot<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => AndNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext AndNot<TPredicate>(Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => AndNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext AndNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    #endregion

    #region Or

    public PredicationBuilderNext Or<TPredicate>()
        where TPredicate : IPredicate
        => Or(typeof(TPredicate), []);

    public PredicationBuilderNext Or<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => Or(typeof(TPredicate), parameters);

    public PredicationBuilderNext Or<TPredicate>(Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => Or(typeof(TPredicate), parameters);

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

    public PredicationBuilderNext OrNot<TPredicate>()
        where TPredicate : IPredicate
        => OrNot(typeof(TPredicate), []);

    public PredicationBuilderNext OrNot<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => OrNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext OrNot<TPredicate>(Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => OrNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext OrNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return new(this);
    }

    #endregion

    #region Xor

    public PredicationBuilderNext Xor<TPredicate>()
        where TPredicate : IPredicate
        => Xor(typeof(TPredicate), []);

    public PredicationBuilderNext Xor<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => Xor(typeof(TPredicate), parameters);

    public PredicationBuilderNext Xor<TPredicate>(Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => Xor(typeof(TPredicate), parameters);

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

    public PredicationBuilderNext XorNot<TPredicate>()
        where TPredicate : IPredicate
        => XorNot(typeof(TPredicate), []);

    public PredicationBuilderNext XorNot<TPredicate>(params object?[] parameters)
        where TPredicate : IPredicate
        => XorNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext XorNot<TPredicate>(Expression<Func<IContext, object?>>[] parameters)
        where TPredicate : IPredicate
        => XorNot(typeof(TPredicate), parameters);

    public PredicationBuilderNext XorNot(Type type, params object?[] parameters)
    {
        var right = BuildNot(type, parameters);
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return new(this);
    }

    #endregion
}
