using Expressif.Functions;
using Expressif.Values;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public class ParameterParser
{
    protected static readonly Parser<IParameter> VariableParser =
        from name in Grammar.Variable
        select new VariableParameter(name);

    protected static readonly Parser<IParameter> ItemParser =
        from _ in Parse.Char('[').Token()
        from name in Grammar.Literal
        from _1 in Parse.Char(']').Token()
        select new ObjectPropertyParameter(name);

    protected static readonly Parser<IParameter> IndexParser =
        from _ in Parse.Char('#')
        from index in Parse.Number
        select new ObjectIndexParameter(int.Parse(index));

    protected static readonly Parser<IParameter> ParametrizedExpressionParser =
        from _ in Parse.Char('{').Token()
        from expression in ExpressionApplicableParser.Parser
        from _1 in Parse.Char('}').Token()
        select new InputExpressionParameter(expression);

    protected static readonly Parser<IParameter> LiteralParser =
        from name in Grammar.Literal
        select new LiteralParameter(name);

    protected static readonly Parser<IParameter> IntervalParser =
        from interval in Parsers.IntervalParser.Parser
        select new IntervalParameter(interval);

    public static readonly Parser<IParameter> Parser = 
        VariableParser
        .Or(IntervalParser)
        .Or(IndexParser)
        .Or(ItemParser)
        .Or(ParametrizedExpressionParser)
        .Or(LiteralParser)
        ;
}

public class ParametersParser
{
    public static readonly Parser<IParameter[]> Parser =
        from _ in Parse.Char('(').Token()
        from first in ParameterParser.Parser.Once()
        from others in (
            from _ in Parse.Char(',').Token()
            from p in ParameterParser.Parser.Token()
            select p).Many()
        from _1 in Parse.Char(')').Token()
        select first.Concat(others).ToArray();
}
