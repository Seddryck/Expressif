using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Expressif.Predicates;

public class PredicationFactory : BaseExpressionFactory
{
    private Parser<IPredicationParsable> Parser { get; } = PredicationParser.Parser;

    protected UnaryOperatorFactory<IPredicate> UnaryOperatorFactory { get; }
    protected BinaryOperatorFactory<IPredicate> BinaryOperatorFactory { get; }

    protected internal PredicationFactory(PredicateTypeMapper mapper, UnaryOperatorFactory<IPredicate> unary, BinaryOperatorFactory<IPredicate> binary)
        : base(mapper)
        => (UnaryOperatorFactory, BinaryOperatorFactory) = (unary, binary);

    public PredicationFactory()
        : this(new PredicateTypeMapper(), new UnaryOperatorFactory<IPredicate>(), new BinaryOperatorFactory<IPredicate>()) { }

    public virtual IPredicate Instantiate(string code, IContext context)
    {
        var predication = Parser.Parse(code);
        var predicate = Instantiate(predication, context);
        return predicate;
    }

    public virtual IPredicate Instantiate(IPredicationParsable predication, IContext context)
    => predication switch
    {
        SinglePredicationMeta single => Instantiate(single, context),
        UnaryPredicationMeta unary => Instantiate(unary, context),
        BinaryPredicationMeta binary => Instantiate(binary, context),
        _ => throw new NotImplementedException()
    };

    internal virtual IPredicate Instantiate(SinglePredicationMeta basic, IContext context)
        => (Instantiate<IPredicate>(basic.Member.Name, basic.Member.Parameters, context));

    internal IPredicate Instantiate(UnaryPredicationMeta unary, IContext context)
    {
        var predicate = Instantiate(unary.Member, context);
        return UnaryOperatorFactory.Instantiate(unary.Operator.Name, predicate);
    }

    internal IPredicate Instantiate(BinaryPredicationMeta binary, IContext context)
    {
        var left = Instantiate(binary.Left, context);
        var right = Instantiate(binary.Right, context);
        return BinaryOperatorFactory.Instantiate(binary.Operator.Name, left, right);
    }
}
