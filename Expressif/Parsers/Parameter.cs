using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public interface IParameter
    { }

    public record class LiteralParameter (string Value) : IParameter { }
    public record class VariableParameter(string Name) : IParameter { }
    public record class ObjectPropertyParameter(string Name) : IParameter { }
    public record class ObjectIndexParameter(int Index) : IParameter { }

    public record class ParameterizedExpressionParameter(ParametrizedExpression Expression) : IParameter { }

    public class Parameter
    {
        private static readonly Parser<IParameter> VariableParameter =
            from name in Grammar.Variable
            select new VariableParameter(name);
        
        private static readonly Parser<IParameter> ItemParameter =
            from _ in Parse.Char('[').Token()
            from name in Grammar.Literal
            from _1 in Parse.Char(']').Token()
            select new ObjectPropertyParameter(name);

        private static readonly Parser<IParameter> IndexParameter =
            from _ in Parse.Char('#')
            from index in Parse.Number
            select new ObjectIndexParameter(int.Parse(index));

        private static readonly Parser<IParameter> ParametrizedExpressionParameter =
            from _ in Parse.Char('{').Token()
            from expression in ParametrizedExpression.Parser
            from _1 in Parse.Char('}').Token()
            select new ParameterizedExpressionParameter(expression);

        private static readonly Parser<IParameter> LiteralParameter =
            from name in Grammar.Literal
            select new LiteralParameter(name);


        public static readonly Parser<IParameter> Parser = 
            VariableParameter
            .Or(IndexParameter)
            .Or(ItemParameter)
            .Or(ParametrizedExpressionParameter)
            .Or(LiteralParameter)
            ;
    }

    public class Parameters
    {
        public static readonly Parser<IParameter[]> Parser =
            from _ in Parse.Char('(').Token()
            from first in Parameter.Parser.Once()
            from others in (
                from _ in Parse.Char(',').Token()
                from p in Parameter.Parser.Token()
                select p).Many()
            from _1 in Parse.Char(')').Token()
            select first.Concat(others).ToArray();
    }
}
