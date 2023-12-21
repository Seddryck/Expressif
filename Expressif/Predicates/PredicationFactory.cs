using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates;

public class PredicationFactory : BaseExpressionFactory
{
    private Parser<IPredication> Parser { get; } = Parsers.Predication.Parser;

    protected UnaryOperatorFactory UnaryOperatorFactory { get; }
    protected BinaryOperatorFactory BinaryOperatorFactory { get; }

    protected internal PredicationFactory(PredicateTypeMapper mapper, UnaryOperatorFactory unary, BinaryOperatorFactory binary)
        : base(mapper)
        => (UnaryOperatorFactory, BinaryOperatorFactory) = (unary, binary);

    public PredicationFactory()
        : this(new PredicateTypeMapper(), new UnaryOperatorFactory(), new BinaryOperatorFactory()) { }

    public virtual IPredicate Instantiate(string code, Context context)
    {
        var predication = Parser.Parse(code);
        var predicate = Instantiate(predication, context);
        return predicate;
    }

    public IPredicate Instantiate(IPredication predication, Context context)
    => predication switch
    {
        SinglePredication single => Instantiate(single, context),
        UnaryPredication unary => Instantiate(unary, context),
        BinaryPredication binary => Instantiate(binary, context),
        _ => throw new NotImplementedException()
    };

    internal IPredicate Instantiate(SinglePredication basic, Context context)
    {
        var predicates = new List<IPredicate>();
        foreach (var predicate in basic.Members)
            predicates.Add(Instantiate<IPredicate>(predicate.Name, predicate.Parameters, context));
        return predicates[0];
    }

    internal IPredicate Instantiate(UnaryPredication unary, Context context)
    {
        var predicate = Instantiate(unary.Member, context);
        return UnaryOperatorFactory.Instantiate(unary.Operator.Name, predicate);
    }

    internal IPredicate Instantiate(BinaryPredication binary, Context context)
    {
        var left = Instantiate(binary.LeftMember, context);
        var right = Instantiate(binary.RightMember, context);
        return BinaryOperatorFactory.Instantiate(binary.Operator.Name, left, right);
    }
}
