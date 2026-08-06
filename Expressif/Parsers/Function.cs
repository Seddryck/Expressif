using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expressif.Parsers;

public enum FunctionSyntax
{
    Standard,
    MapShorthand,
    FieldShorthand
}

public class Function : IExpression
{
    private static readonly HashSet<string> PredicateParameterFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "filter",
        "none",
        "all",
        "some",
        "single",
        "any",
        "every",
    };

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

    private static readonly Parser<Function> FieldShorthandParser =
        from _ in Parse.Char('.')
        from name in Parse.Regex("[A-Za-z_][A-Za-z0-9_+\\-]*").Text()
        select new Function("field", [new LiteralParameter(name)], FunctionSyntax.FieldShorthand);

    private static readonly Parser<Function> TupleProjectionShorthandParser =
        from _ in Parse.Char('$')
        from index in Parse.Number
        select new Function("tuple-at", [new LiteralParameter(index)]);

    private static readonly Parser<IParameter[]> FieldShorthandParametersParser =
        from function in FieldShorthandParser.Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
        select new IParameter[] { new OpenExpressionParameter(new OpenExpression([function])) };

    private static readonly Parser<Function> StandardParser =
        from functionName in Grammar.FunctionName
        from parameters in (PredicateParameterFunctions.Contains(functionName)
                                ? FieldShorthandParametersParser.Or(PredicationParametersParser).Optional()
                                : functionName.Equals("map", StringComparison.OrdinalIgnoreCase)
                                  || functionName.Equals("adjacent", StringComparison.OrdinalIgnoreCase)
                                ? OpenExpressionParametersParser.Optional()
                                : functionName.Equals("record", StringComparison.OrdinalIgnoreCase)
                                ? RecordParametersParser.Optional()
                                : Parsers.Parameters.Parser.Optional())
        select new Function(functionName, parameters.GetOrElse(Array.Empty<IParameter>()));

    public static readonly Parser<Function> Parser =
        TupleProjectionShorthandParser.Or(FieldShorthandParser).Or(StandardParser);

    public string Name { get; }
    public IParameter[] Parameters { get; }
    public FunctionSyntax Syntax { get; }

    public Function(string name, IParameter[] parameters, FunctionSyntax syntax = FunctionSyntax.Standard)
        => (Name, Parameters, Syntax) = (name, parameters, syntax);
}
