using Expressif.Functions;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Expressif.Parsers;

public interface IExpressionParsable { }

public class ExpressionParser
{
    public static readonly Parser<ExpressionMeta> Parser =
        from first in FunctionParser.Parser.Once()
        from others in (
            from _ in Parse.Char('|').Token()
            from p in FunctionParser.Parser.Token()
            select p).Many()
        select new ExpressionMeta(first.Concat(others).ToArray());
}

public class ExpressionApplicableParser
{
    public static readonly Parser<ExpressionApplicableMeta> Parser =
        from parameter in ParameterParser.Parser.Token()
        from remaining in (
            from _ in Parse.Char('|').Token()
            from expression in ExpressionParser.Parser
            select expression.Members
        ).Optional()
        select new ExpressionApplicableMeta(parameter, remaining.GetOrElse([]));
}
