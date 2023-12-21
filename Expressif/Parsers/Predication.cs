using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Predicates.Boolean;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public interface IPredication { }

public class Predication : IPredication
{
    public static readonly Parser<IPredication> Parser =
        UnaryPredication.Parser
            .Or(BinaryPredication.Parser)
            .Or(SubPredication.Parser)
            .Or(SinglePredication.Parser);
}

public class SinglePredication : IPredication
{
    public Function[] Members { get; }
    
    public SinglePredication(Function[] members)
        => (Members) = (members);

    public SinglePredication(Function member)
        : this([member]) { }

    public static readonly Parser<IPredication> Parser =
        from predicate in Function.Parser
        select new SinglePredication([predicate]);
}

internal class UnaryPredication : IPredication
{
    public UnaryOperator Operator { get; }
    public IPredication Member { get; }

    public UnaryPredication(UnaryOperator @operator, IPredication predication)
        => (Operator, Member) = (@operator, predication);

    public static readonly Parser<IPredication> Parser =
        from unaryOperator in UnaryOperator.Parser
        from predicate in Predication.Parser
        select new UnaryPredication(unaryOperator, predicate);
}

internal class BinaryPredication : IPredication
{
    public IPredication LeftMember { get; }
    public BinaryOperator Operator { get; }
    public IPredication RightMember { get; }

    public BinaryPredication(BinaryOperator @operator, IPredication left, IPredication right)
        => (Operator, LeftMember, RightMember) = (@operator, left, right);

    public static readonly Parser<IPredication> Parser =
        Parse.ChainOperator(BinaryOperator.Parser, SubPredication.Parser.Or(UnaryPredication.Parser).Or(SinglePredication.Parser), (op, left, right)
            => new BinaryPredication(op, left, right));
}

internal class SubPredication : IPredication
{
    public static readonly Parser<IPredication> Parser =
        from open in Parse.Char('{').Token()
        from chain in Predication.Parser
        from close in Parse.Char('}').Token()
        select chain;
}

public class InputPredication
{
    public IPredication Predication { get; }
    public IParameter Parameter { get; }

    public InputPredication(IParameter parameter, IPredication member)
        => (Parameter, Predication) = (parameter, member);

    public static readonly Parser<InputPredication> Parser =
        from parameter in Parsers.Parameter.Parser.Token()
        from _ in Parse.IgnoreCase("|?").Token()
        from predication in Parsers.Predication.Parser.Token()
        select new InputPredication(parameter, predication);
}
