using Expressif.Predicates;
using Expressif.Values.Special;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Expressif.Parsers;
using Expressif.Functions;
using Expressif.Predicates.Operators;

namespace Expressif;

public interface IPredicationBuilder
{
    IContext Context { get; }
    PredicationFactory Factory { get; }
    PredicationSerializer Serializer { get; }
    IPredicationParsable? Pile { get; }
}

public class AbstractPredicationBuilder : IPredicationBuilder
{
    public IContext Context { get; }
    public PredicationFactory Factory { get; }
    public PredicationSerializer Serializer { get; }
    public IPredicationParsable? Pile { get; protected set; }

    protected AbstractPredicationBuilder(IContext? context, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new(), serializer ?? new());

    protected AbstractPredicationBuilder(IPredicationBuilder builder)
        => (Context, Factory, Serializer, Pile) = (builder.Context, builder.Factory, builder.Serializer, builder.Pile);

    protected IPredicationParsable CreateNot(Type type, object?[] parameters)
    => new UnaryPredicationMeta(new OperatorMeta("!")
            , new SinglePredicationMeta(new FunctionMeta(type.Name, Parametrize(parameters)))
        );

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

    public virtual IPredicate Build()
    {
        if (Pile is null)
            throw new InvalidOperationException();
        var predicate = Factory.Instantiate(Pile, Context);
        return predicate;
    }

    public string Serialize()
    {
        if (Pile is null)
            throw new InvalidOperationException();

        return Serializer.Serialize(this);
    }
}


public abstract class BaseFirstPredicationBuilder<T> : AbstractPredicationBuilder
{
    public BaseFirstPredicationBuilder(IContext? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        : base(context, factory, serializer) { }

    protected abstract T CreateNext(IPredicationBuilder builder);

    public T Is<P>()
        where P : IPredicate
      => Is(typeof(P), []);

    public T Is<P>(params object?[] parameters)
        where P : IPredicate
      => Is(typeof(P), parameters);

    public T Is<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
       => Is(typeof(P), parameters);

    public T Is(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));

        Pile = new SinglePredicationMeta(new FunctionMeta(type.Name, Parametrize(parameters)));
        return CreateNext(this);
    }

    public T IsNot<P>()
        where P : IPredicate
        => IsNot<P>([]);

    public T IsNot<P>(params object?[] parameters)
        where P : IPredicate
        => IsNot(typeof(P), parameters);

    public T IsNot<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => IsNot(typeof(P), parameters);

    public T IsNot(Type type, params object?[] parameters)
    {
        if (!type.GetInterfaces().Contains(typeof(IPredicate)))
            throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(type));
        
        Pile = CreateNot(type, Parametrize(parameters));
        return CreateNext(this);
    }
}

public class PredicationBuilder : BaseFirstPredicationBuilder<NextPredicationBuilder>
{
    public PredicationBuilder(IContext? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        : base(context, factory, serializer) { }

    protected override NextPredicationBuilder CreateNext(IPredicationBuilder builder)
        => new(this);
}

public abstract class BaseNextPredicationBuilder<T> : AbstractPredicationBuilder
{
    public BaseNextPredicationBuilder(IPredicationBuilder builder)
        : base(builder) { }

    protected abstract T CreateNext(IPredicationBuilder builder);

    #region And

    public T And<P>()
        where P : IPredicate
        => And(typeof(P), []);

    public T And<P>(params object?[] parameters)
        where P : IPredicate
        => And(typeof(P), parameters);

    public T And<P>(params Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => And(typeof(P), parameters);

    public T And(Type type, params object?[] parameters)
    {
        var right = new SinglePredicationMeta(new FunctionMeta(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.And, Pile!, right);
        return CreateNext(this);
    }

    public T And(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.And, Pile!, builder.Pile!);
        return CreateNext(this);
    }

    #endregion

    #region AndNot

    public T AndNot<P>()
        where P : IPredicate
        => AndNot(typeof(P), []);

    public T AndNot<P>(params object?[] parameters)
        where P : IPredicate
        => AndNot(typeof(P), parameters);

    public T AndNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => AndNot(typeof(P), parameters);

    public T AndNot(Type type, params object?[] parameters)
    {
        var right = CreateNot(type, parameters);
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.And, Pile!, right);
        return CreateNext(this);
    }

    #endregion

    #region Or

    public T Or<P>()
        where P : IPredicate
        => Or(typeof(P), []);

    public T Or<P>(params object?[] parameters)
        where P : IPredicate
        => Or(typeof(P), parameters);

    public T Or<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => Or(typeof(P), parameters);

    public T Or(Type type, params object?[] parameters)
    {
        var right = new SinglePredicationMeta(new FunctionMeta(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Or, Pile!, right);
        return CreateNext(this);
    }

    public T Or(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Or, Pile!, builder.Pile!);
        return CreateNext(this);
    }

    #endregion

    #region OrNot

    public T OrNot<P>()
        where P : IPredicate
        => OrNot(typeof(P), []);

    public T OrNot<P>(params object?[] parameters)
        where P : IPredicate
        => OrNot(typeof(P), parameters);

    public T OrNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => OrNot(typeof(P), parameters);

    public T OrNot(Type type, params object?[] parameters)
    {
        var right = CreateNot(type, parameters);
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Or, Pile!, right);
        return CreateNext(this);
    }

    #endregion

    #region Xor

    public T Xor<P>()
        where P : IPredicate
        => Xor(typeof(P), []);

    public T Xor<P>(params object?[] parameters)
        where P : IPredicate
        => Xor(typeof(P), parameters);

    public T Xor<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => Xor(typeof(P), parameters);

    public T Xor(Type type, params object?[] parameters)
    {
        var right = new SinglePredicationMeta(new FunctionMeta(type.Name, Parametrize(parameters)));
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Xor, Pile!, right);
        return CreateNext(this);
    }

    public T Xor(AbstractPredicationBuilder builder)
    {
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Xor, Pile!, builder.Pile!);
        return CreateNext(this);
    }

    #endregion

    #region XorNot

    public T XorNot<P>()
        where P : IPredicate
        => XorNot(typeof(P), []);

    public T XorNot<P>(params object?[] parameters)
        where P : IPredicate
        => XorNot(typeof(P), parameters);

    public T XorNot<P>(Expression<Func<IContext, object?>>[] parameters)
        where P : IPredicate
        => XorNot(typeof(P), parameters);

    public T XorNot(Type type, params object?[] parameters)
    {
        var right = CreateNot(type, parameters);
        Pile = new BinaryPredicationMeta(BinaryOperatorParser.Xor, Pile!, right);
        return CreateNext(this);
    }

    #endregion
}

public class NextPredicationBuilder : BaseNextPredicationBuilder<NextPredicationBuilder>
{
    public NextPredicationBuilder(IPredicationBuilder builder)
        : base(builder) { }

    protected override NextPredicationBuilder CreateNext(IPredicationBuilder builder)
        => new(this);
}
