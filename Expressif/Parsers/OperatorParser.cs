using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Predicates.Boolean;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public class UnaryOperatorParser
{
    public static readonly Parser<OperatorMeta> Parser =
        from op in Parse.Char('!').Token()
        select new OperatorMeta(op.ToString());
}

public class BinaryOperatorParser
{
    public static readonly Parser<OperatorMeta> Parser =
        from op in Parse.Char('|').Token()
        from @operator in Keyword.OrOperator.Or(Keyword.AndOperator).Or(Keyword.XorOperator)
        select new OperatorMeta(@operator);

    public static OperatorMeta And => new("And");
    public static OperatorMeta Or => new("Or");
    public static OperatorMeta Xor => new("Xor");
}
