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

namespace Expressif.Parsers
{
    internal class UnaryOperator : IPredication
    {
        public string Name { get; }
        
        public UnaryOperator(string name)
            => (Name) = (name);

        public static readonly Parser<UnaryOperator> Parser =
            from op in Parse.Char('!').Token()
            select new UnaryOperator(op.ToString());
    }

    internal class BinaryOperator : IPredication
    {
        public string Name { get; }
        public BinaryOperator(string name)
            => (Name) = (name);

        public static readonly Parser<BinaryOperator> Parser =
            from op in Parse.Char('|').Token()
            from @operator in Keyword.OrOperator.Or(Keyword.AndOperator).Or(Keyword.XorOperator)
            select new BinaryOperator(@operator);

        public static BinaryOperator And => new BinaryOperator("And");
        public static BinaryOperator Or => new BinaryOperator("Or");
        public static BinaryOperator Xor => new BinaryOperator("Xor");
    }
}
