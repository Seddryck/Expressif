using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public class PredicationParser
{
    public static readonly Parser<IPredicationParsable> Parser =
        UnaryPredicationParser.Parser
            .Or(BinaryPredicationParser.Parser)
            .Or(SubPredicationParser.Parser)
            .Or(SinglePredicationParser.Parser);
}

public class SinglePredicationParser
{
    public static readonly Parser<SinglePredicationMeta> Parser =
        from predicate in FunctionParser.Parser
        select new SinglePredicationMeta(predicate);
}

internal class UnaryPredicationParser
{
    public static readonly Parser<IPredicationParsable> Parser =
        from unaryOperator in UnaryOperatorParser.Parser
        from predicate in PredicationParser.Parser
        select new UnaryPredicationMeta(unaryOperator, predicate);
}

internal class BinaryPredicationParser
{
    public static readonly Parser<IPredicationParsable> Parser =
        Parse.ChainOperator(BinaryOperatorParser.Parser, SubPredicationParser.Parser.Or(UnaryPredicationParser.Parser).Or(SinglePredicationParser.Parser), (op, left, right)
            => new BinaryPredicationMeta(op, left, right));
}

internal class SubPredicationParser
{
    public static readonly Parser<IPredicationParsable> Parser =
        from open in Parse.Char('{').Token()
        from chain in PredicationParser.Parser
        from close in Parse.Char('}').Token()
        select chain;
}

public class PredicationApplicableParser
{
    public static readonly Parser<PredicationApplicableMeta> Parser =
        from parameter in ParameterParser.Parser.Token()
        from _ in Parse.IgnoreCase("|?").Token()
        from predication in PredicationParser.Parser.Token()
        select new PredicationApplicableMeta(parameter, predication);
}
