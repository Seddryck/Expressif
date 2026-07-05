using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers;

public class Function : IExpression
{
    private static readonly Parser<IParameter[]> MapParametersParser =
        from _ in Parse.Char('(').Token()
        from expression in OpenExpression.Parser.Token()
        from _1 in Parse.Char(')').Token()
        select new IParameter[] { new OpenExpressionParameter(expression) };

    public static readonly Parser<Function> Parser =
        from functionName in Grammar.FunctionName
        from parameters in (functionName.Equals("map", StringComparison.OrdinalIgnoreCase)
                                ? MapParametersParser.Optional()
                                : Parsers.Parameters.Parser.Optional())
        select new Function(functionName, parameters.GetOrElse(Array.Empty<IParameter>()));

    public string Name { get; }
    public IParameter[] Parameters { get; }

    public Function(string name, IParameter[] parameters)
        => (Name, Parameters) = (name, parameters);
}
