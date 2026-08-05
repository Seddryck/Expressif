using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expressif.Parsers;

public enum FunctionSyntax
{
    Standard,
    MapShorthand
}

public class Function : IExpression
{
    private static readonly Parser<IParameter[]> OpenExpressionParametersParser =
        from _ in Parse.Char('(').Token()
        from expression in OpenExpression.Parser.Token()
        from _1 in Parse.Char(')').Token()
        select new IParameter[] { new OpenExpressionParameter(expression) };

    private static readonly Parser<IParameter[]> PredicationParametersParser =
        from _ in Parse.Char('(').Token()
        from predication in Predication.Parser.Token()
        from _1 in Parse.Char(')').Token()
        select new IParameter[] { new PredicationParameter(predication) };

    private static readonly Parser<IParameter[]> RecordParametersParser =
        from _ in Parse.Char('(').Token()
        from entries in (
            from close in Parse.Char(')').Token()
            select Array.Empty<IRecordDefinitionEntry>()
        ).Or(
            from first in Parameter.RecordFunctionEntryParser.Token().Once()
            from others in (
                from __ in Parse.Char(',').Token()
                from entry in Parameter.RecordFunctionEntryParser.Token()
                select entry
            ).Many()
            from trailing in Parse.Char(',').Token().Optional()
            from close in Parse.Char(')').Token()
            select first.Concat(others).ToArray()
        )
        select entries.Length == 0
            ? Array.Empty<IParameter>()
            : [new RecordDefinitionParameter(entries)];

    public static readonly Parser<Function> Parser =
        from functionName in Grammar.FunctionName
        from parameters in (functionName.Equals("filter", StringComparison.OrdinalIgnoreCase)
                                ? PredicationParametersParser.Optional()
                                : functionName.Equals("map", StringComparison.OrdinalIgnoreCase)
                                ? OpenExpressionParametersParser.Optional()
                                : functionName.Equals("record", StringComparison.OrdinalIgnoreCase)
                                ? RecordParametersParser.Optional()
                                : Parsers.Parameters.Parser.Optional())
        select new Function(functionName, parameters.GetOrElse(Array.Empty<IParameter>()));

    public string Name { get; }
    public IParameter[] Parameters { get; }
    public FunctionSyntax Syntax { get; }

    public Function(string name, IParameter[] parameters, FunctionSyntax syntax = FunctionSyntax.Standard)
        => (Name, Parameters, Syntax) = (name, parameters, syntax);
}
