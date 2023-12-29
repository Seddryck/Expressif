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

namespace Expressif;

public class PredicationBuilder
{
    private IContext Context { get; }
    private PredicationFactory Factory { get; }
    private PredicationSerializer Serializer { get; }
    
    public PredicationBuilder()
        : this(new Context()) { }
    public PredicationBuilder(IContext? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new(), serializer ?? new());

    private IPredication? Pile { get; set; }

    public PredicationBuilder Create<P>(params object?[] parameters)
        where P : IPredicate
    {
        Pile = new SinglePredication(new Function(typeof(P).Name, Parametrize(parameters)));
        return this;
    }

    private UnaryPredication BuildNot<P>(object?[] parameters)
        => new (new UnaryOperator("!") 
                , new SinglePredication(new Function(typeof(P).Name, Parametrize(parameters)))
            );

    public PredicationBuilder Not<P>(params object?[] parameters)
        where P : IPredicate
    {
        Pile = BuildNot<P>(parameters);
        return this;
    }

    public PredicationBuilder And<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = new SinglePredication(new Function(typeof(P).Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    public PredicationBuilder And(PredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilder AndNot<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = BuildNot<P>(parameters);
        Pile = new BinaryPredication(new BinaryOperator("And"), Pile!, right);
        return this;
    }

    public PredicationBuilder Or<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = new SinglePredication(new Function(typeof(P).Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return this;
    }

    public PredicationBuilder Or(PredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilder OrNot<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = BuildNot<P>(parameters);
        Pile = new BinaryPredication(new BinaryOperator("Or"), Pile!, right);
        return this;
    }

    public PredicationBuilder Xor<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = new SinglePredication(new Function(typeof(P).Name, Parametrize(parameters)));
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return this;
    }

    public PredicationBuilder Xor(PredicationBuilder builder)
    {
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, builder.Pile!);
        return this;
    }

    public PredicationBuilder XorNot<P>(params object?[] parameters)
        where P : IPredicate
    {
        var right = BuildNot<P>(parameters);
        Pile = new BinaryPredication(new BinaryOperator("Xor"), Pile!, right);
        return this;
    }

    public IPredicate Build()
    {
        if (Pile is null)
            throw new InvalidOperationException();
        var predicate = Factory.Instantiate(Pile, Context);
        return predicate;
    }

    private IParameter[] Parametrize(object?[] parameters)
    {
        var typedParameters = new List<IParameter>();
        foreach (var parameter in parameters)
        {
            typedParameters.Add(parameter switch
            {
                IParameter p => p,
                _ => new LiteralParameter(parameter?.ToString() ?? new Null().Keyword)
            });
        }
        return typedParameters.ToArray();
    }

    public string Serialize()
    {
        if (Pile is null)
            throw new InvalidOperationException();

        return Serializer.Serialize(Pile);
    }
}
