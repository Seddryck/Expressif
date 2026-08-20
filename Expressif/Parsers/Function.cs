using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;

namespace Expressif.Parsers;

public enum FunctionSyntax
{
    Standard,
    MapShorthand,
    FieldShorthand,
    TupleProjectionShorthand
}

public class Function : IExpression
{
    // A bare token is structurally indistinguishable from a literal. Preserve the
    // established map(function) shorthand while generic parameters handle every
    // expression whose syntax is unambiguous.
    private static readonly Parser<IParameter[]> MapParametersParser =
        from openingParenthesis in Parse.Char('(').Token()
        from expression in OpenExpression.Parser.Token()
        from closingParenthesis in Parse.Char(')').Token()
        select new IParameter[] { new OpenExpressionParameter(expression) };

    private static readonly Parser<IParameter[]> RecordParametersParser =
        from openingParenthesis in Parse.Char('(').Token()
        from entries in (
            from close in Parse.Char(')').Token()
            select Array.Empty<IRecordDefinitionEntry>()
        ).Or(
            from first in Parameter.RecordFunctionEntryParser.Token().Once()
            from others in (
                from comma in Parse.Char(',').Token()
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

    private static readonly Parser<GenerateNamedEntry> GenerateWhileEntryParser =
        from name in Parse.String("while").Text().Token()
        from _ in Parse.String(":=").Token()
        from predication in Predication.Parser.Token()
        select new GenerateNamedEntry(name, new PredicationParameter(predication));

    private static readonly Parser<GenerateNamedEntry> GenerateExpressionEntryParser =
        from name in Parse.String("next").Text().Or(Parse.String("result").Text()).Token()
        from _ in Parse.String(":=").Token()
        from expression in OpenExpression.Parser.Token()
        select new GenerateNamedEntry(name, new OpenExpressionParameter(expression));

    private static readonly Parser<IParameter[]> GenerateParametersParser =
        from _ in Parse.Char('(').Token()
        from first in GenerateWhileEntryParser.Or(GenerateExpressionEntryParser).Once()
        from others in (
            from __ in Parse.Char(',').Token()
            from entry in GenerateWhileEntryParser.Or(GenerateExpressionEntryParser)
            select entry).Many()
        from _1 in Parse.Char(')').Token()
        select new IParameter[] { new GenerateDefinitionParameter(first.Concat(others).ToArray()) };

    private static readonly Parser<Function> FieldShorthandParser =
        from dot in Parse.Char('.')
        from name in Parse.Regex("[A-Za-z_][A-Za-z0-9_+\\-]*").Text()
        select new Function("field", [new LiteralParameter(name)], FunctionSyntax.FieldShorthand);

    private static readonly Parser<Function> TupleProjectionShorthandParser =
        from dollarSign in Parse.Char('$')
        from index in Parse.Number
        select new Function("tuple-at", [new LiteralParameter(index)], FunctionSyntax.TupleProjectionShorthand);

    private static readonly Parser<Function> StandardParser =
        from functionName in Grammar.FunctionName
        from parameters in (functionName.Equals("map", StringComparison.OrdinalIgnoreCase)
                                ? MapParametersParser.Optional()
                                : functionName.Equals("record", StringComparison.OrdinalIgnoreCase)
                                ? RecordParametersParser.Optional()
                                : functionName.Equals("generate", StringComparison.OrdinalIgnoreCase)
                                ? GenerateParametersParser.Optional()
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
